using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ProductApp.Application.SmartProduct;
using ProductApp.Domain.SmartProduct;

namespace ProductApp.Infrastructure.SmartProduct;

public sealed class OfficeTextDocumentExtractor : IAiDocumentExtractor
{
    private static readonly string[] Extensions = [".txt", ".md", ".csv", ".docx", ".xlsx", ".pdf"];
    public bool Supports(string extension) => Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase);

    public async Task<IReadOnlyList<ExtractedDocumentPage>> ExtractAsync(Stream content, string extension, CancellationToken ct)
    {
        extension = extension.ToLowerInvariant();
        if (extension is ".txt" or ".md" or ".csv")
        {
            using var reader = new StreamReader(content, Encoding.UTF8, true, leaveOpen: true);
            return [new ExtractedDocumentPage(null, await reader.ReadToEndAsync(ct))];
        }
        if (extension == ".docx") return ExtractDocx(content);
        if (extension == ".xlsx") return ExtractXlsx(content);
        if (extension == ".pdf") return await ExtractPdfAsync(content, ct);
        return [];
    }

    private static IReadOnlyList<ExtractedDocumentPage> ExtractDocx(Stream content)
    {
        using var archive = new ZipArchive(content, ZipArchiveMode.Read, true);
        var entry = archive.GetEntry("word/document.xml") ?? throw new AiAttachmentValidationException("Document Word invalide.");
        using var stream = entry.Open(); var document = XDocument.Load(stream);
        XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        var text = string.Join(Environment.NewLine, document.Descendants(w + "p")
            .Select(paragraph => string.Concat(paragraph.Descendants(w + "t").Select(node => node.Value))));
        return [new ExtractedDocumentPage(null, text)];
    }

    private static IReadOnlyList<ExtractedDocumentPage> ExtractXlsx(Stream content)
    {
        using var archive = new ZipArchive(content, ZipArchiveMode.Read, true);
        var shared = new List<string>();
        var sharedEntry = archive.GetEntry("xl/sharedStrings.xml");
        if (sharedEntry is not null)
        {
            using var stream = sharedEntry.Open(); var document = XDocument.Load(stream);
            shared.AddRange(document.Descendants().Where(node => node.Name.LocalName == "si")
                .Select(item => string.Concat(item.Descendants().Where(node => node.Name.LocalName == "t").Select(node => node.Value))));
        }
        var pages = new List<ExtractedDocumentPage>();
        foreach (var entry in archive.Entries.Where(entry => entry.FullName.StartsWith("xl/worksheets/sheet", StringComparison.OrdinalIgnoreCase)
            && entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)).OrderBy(entry => entry.FullName))
        {
            using var stream = entry.Open(); var document = XDocument.Load(stream); var rows = new List<string>();
            foreach (var row in document.Descendants().Where(node => node.Name.LocalName == "row"))
            {
                var cells = row.Elements().Where(node => node.Name.LocalName == "c").Select(cell =>
                {
                    var raw = cell.Descendants().FirstOrDefault(node => node.Name.LocalName == "v")?.Value ?? string.Empty;
                    return cell.Attribute("t")?.Value == "s" && int.TryParse(raw, out var index) && index < shared.Count ? shared[index] : raw;
                });
                rows.Add(string.Join(" | ", cells));
            }
            pages.Add(new ExtractedDocumentPage(pages.Count + 1, string.Join(Environment.NewLine, rows)));
        }
        return pages;
    }

    private static async Task<IReadOnlyList<ExtractedDocumentPage>> ExtractPdfAsync(Stream content, CancellationToken ct)
    {
        using var memory = new MemoryStream(); await content.CopyToAsync(memory, ct);
        var binary = Encoding.Latin1.GetString(memory.ToArray());
        var matches = Regex.Matches(binary, @"\((?<text>(?:\\.|[^\\)]){3,})\)\s*(?:Tj|TJ)");
        var text = string.Join(Environment.NewLine, matches.Select(match => Regex.Unescape(match.Groups["text"].Value)));
        if (string.IsNullOrWhiteSpace(text)) throw new AiAttachmentValidationException("Ce PDF ne contient pas de texte extractible. Un adaptateur OCR pourra être ajouté.");
        return [new ExtractedDocumentPage(null, text)];
    }
}

public sealed class AiKnowledgeService(IAiConversationRepository repository, IAiAttachmentStorage storage,
    IAiDocumentExtractor extractor, IAiProvider provider, TimeProvider timeProvider) : IAiKnowledgeService
{
    public async Task IndexAsync(AiAttachment attachment, AiConversation conversation, CancellationToken ct)
    {
        var extension = Path.GetExtension(attachment.FileName);
        if (!extractor.Supports(extension)) { attachment.MarkFailed("document_extraction_not_supported", UtcNow()); await repository.SaveChangesAsync(ct); return; }
        attachment.BeginIndexing(UtcNow()); await repository.SaveChangesAsync(ct);
        try
        {
            await using var content = await storage.OpenReadAsync(attachment.StorageKey, ct);
            var pages = await extractor.ExtractAsync(content, extension, ct);
            var chunks = new List<AiDocumentChunk>(); var index = 0;
            foreach (var page in pages)
            {
                foreach (var text in Chunk(page.Text, 1600, 220))
                {
                    if (chunks.Count >= 200) break;
                    var embedding = await SafeEmbeddingAsync(text, ct);
                    chunks.Add(AiDocumentChunk.Create(attachment.Id, conversation.Id, attachment.UserId,
                        conversation.ProductId, conversation.ExperimentId, page.PageNumber, index++, attachment.FileName,
                        text, embedding is null ? null : JsonSerializer.Serialize(embedding), UtcNow()));
                }
                if (chunks.Count >= 200) break;
            }
            if (chunks.Count == 0) throw new AiAttachmentValidationException("Aucun texte exploitable n’a été détecté.");
            await repository.ReplaceChunksAsync(attachment.Id, chunks, ct);
            attachment.MarkIndexed(UtcNow()); await repository.SaveChangesAsync(ct);
        }
        catch (Exception) when (!ct.IsCancellationRequested)
        {
            attachment.MarkFailed("document_indexing_failed", UtcNow()); await repository.SaveChangesAsync(CancellationToken.None);
        }
    }

    public async Task<IReadOnlyList<AiDocumentSearchResult>> SearchAsync(Guid userId, Guid conversationId,
        string query, int maximumResults, CancellationToken ct)
    {
        var chunks = await repository.GetAuthorizedChunksAsync(userId, conversationId, ct);
        if (chunks.Count == 0) return [];
        var queryEmbedding = await SafeEmbeddingAsync(query, ct);
        var terms = Words(query);
        return chunks.Select(chunk => new
            {
                Chunk = chunk,
                Score = Score(queryEmbedding, chunk.EmbeddingJson, terms, chunk.Content)
            })
            .Where(item => item.Score > 0).OrderByDescending(item => item.Score).Take(Math.Clamp(maximumResults, 1, 20))
            .Select(item => new AiDocumentSearchResult(item.Chunk.AttachmentId, item.Chunk.Attachment.FileName,
                item.Chunk.PageNumber, item.Chunk.ChunkIndex, item.Chunk.Content, item.Score)).ToArray();
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
    private async Task<IReadOnlyList<float>?> SafeEmbeddingAsync(string text, CancellationToken ct)
    {
        try { return await provider.CreateEmbeddingAsync(text, ct); }
        catch (Exception) when (!ct.IsCancellationRequested) { return null; }
    }
    private static IEnumerable<string> Chunk(string text, int size, int overlap)
    {
        var normalized = Regex.Replace(text ?? string.Empty, @"\s+", " ").Trim();
        for (var start = 0; start < normalized.Length; start += size - overlap)
            yield return normalized.Substring(start, Math.Min(size, normalized.Length - start));
    }
    private static HashSet<string> Words(string text) => Regex.Matches(text.ToLowerInvariant(), @"[\p{L}\p{N}]{3,}")
        .Select(match => match.Value).ToHashSet();
    private static double Lexical(HashSet<string> terms, string content)
    {
        if (terms.Count == 0) return 0; var words = Words(content); return terms.Count(term => words.Contains(term)) / (double)terms.Count;
    }
    private static double Score(IReadOnlyList<float>? queryEmbedding, string? embeddingJson,
        HashSet<string> terms, string content)
    {
        if (queryEmbedding is null || string.IsNullOrWhiteSpace(embeddingJson))
            return Lexical(terms, content);
        var storedEmbedding = JsonSerializer.Deserialize<float[]>(embeddingJson) ?? [];
        return queryEmbedding.Count == storedEmbedding.Length
            ? Cosine(queryEmbedding, storedEmbedding)
            : Lexical(terms, content);
    }
    private static double Cosine(IReadOnlyList<float> left, IReadOnlyList<float> right)
    {
        if (left.Count == 0 || left.Count != right.Count) return 0;
        double dot = 0, a = 0, b = 0; for (var i = 0; i < left.Count; i++) { dot += left[i] * right[i]; a += left[i] * left[i]; b += right[i] * right[i]; }
        return a == 0 || b == 0 ? 0 : dot / (Math.Sqrt(a) * Math.Sqrt(b));
    }
}
