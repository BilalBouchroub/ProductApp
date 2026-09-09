using ProductApp.Domain.Common;
using System.Text.Json;

namespace ProductApp.Domain.Commercial;

public sealed class OptimizationRequest : AuditableEntity
{
    private OptimizationRequest() { }
    public Guid Id { get; private set; }
    public Guid MarketStudyId { get; private set; }
    public MarketStudy MarketStudy { get; private set; } = null!;
    public string Message { get; private set; } = string.Empty;
    public OptimizationPriority Priority { get; private set; }
    public string RequestedChangesJson { get; private set; } = "[]";
    public OptimizationStatus Status { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public string? Resolution { get; private set; }

    public IReadOnlyList<string> RequestedChanges =>
        JsonSerializer.Deserialize<string[]>(RequestedChangesJson) ?? [];

    public static OptimizationRequest Create(MarketStudy marketStudy, string message,
        OptimizationPriority priority, IEnumerable<string> requestedChanges,
        DateTime? createdAt = null)
    {
        ArgumentNullException.ThrowIfNull(marketStudy);
        if (marketStudy.Status != MarketStudyStatus.Validated)
            throw new InvalidOperationException("L’étude doit être validée avant de demander une optimisation.");
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Le message est obligatoire.", nameof(message));
        var changes = requestedChanges.Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (changes.Length == 0)
            throw new ArgumentException("Au moins une modification est obligatoire.", nameof(requestedChanges));

        var request = new OptimizationRequest
        {
            Id = Guid.NewGuid(),
            MarketStudyId = marketStudy.Id,
            MarketStudy = marketStudy,
            Message = message.Trim(),
            Priority = priority,
            RequestedChangesJson = JsonSerializer.Serialize(changes),
            Status = OptimizationStatus.New
        };
        request.InitializeAudit(createdAt ?? DateTime.UtcNow);
        return request;
    }
}
