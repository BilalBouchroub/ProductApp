using System.Security.Cryptography;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProductApp.Application.SmartProduct;

namespace ProductApp.Infrastructure.SmartProduct;

public sealed class LocalAiAttachmentStorage : IAiAttachmentStorage
{
    private readonly string root;
    public LocalAiAttachmentStorage(IHostEnvironment environment, IOptions<SmartProductOptions> options)
    {
        root = Path.GetFullPath(Path.Combine(environment.ContentRootPath, options.Value.AttachmentRoot));
        Directory.CreateDirectory(root);
    }

    public async Task<StoredAiFile> SaveAsync(Guid userId, Stream content, string safeExtension, CancellationToken ct)
    {
        var userDirectory = Path.Combine(root, userId.ToString("N")); Directory.CreateDirectory(userDirectory);
        var storedName = $"{Guid.NewGuid():N}{safeExtension}";
        var fullPath = EnsureInsideRoot(Path.Combine(userDirectory, storedName));
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        await using var target = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
        var buffer = new byte[81920];
        int read;
        while ((read = await content.ReadAsync(buffer, ct)) > 0)
        {
            hash.AppendData(buffer, 0, read); await target.WriteAsync(buffer.AsMemory(0, read), ct);
        }
        return new StoredAiFile(storedName, Path.GetRelativePath(root, fullPath), Convert.ToHexString(hash.GetHashAndReset()));
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Stream stream = new FileStream(EnsureInsideRoot(Path.Combine(root, storageKey)), FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        return Task.FromResult(stream);
    }
    public Task DeleteAsync(string storageKey, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested(); var path = EnsureInsideRoot(Path.Combine(root, storageKey)); if (File.Exists(path)) File.Delete(path); return Task.CompletedTask;
    }
    private string EnsureInsideRoot(string path)
    {
        var full = Path.GetFullPath(path);
        if (!full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new AiAccessDeniedException();
        return full;
    }
}
