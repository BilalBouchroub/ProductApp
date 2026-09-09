using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ProductApp.Domain.Common;
using ProductApp.Domain.Products;
using ProductApp.Domain.ProductionSteps;
using ProductApp.Domain.Catalog;
using ProductApp.Domain.Commercial;
using ProductApp.Domain.Experiments;
using ProductApp.Domain.Identity;
using ProductApp.Domain.Resources;
using ProductApp.Domain.SmartProduct;
using MarketAnalysisEntity = ProductApp.Domain.MarketAnalysis.MarketAnalysis;

namespace ProductApp.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
    IHttpContextAccessor? httpContextAccessor = null, TimeProvider? timeProvider = null)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVersion> ProductVersions => Set<ProductVersion>();

    public DbSet<ProductionStep> ProductionSteps => Set<ProductionStep>();
    public DbSet<StepResource> StepResources => Set<StepResource>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<RawMaterial> RawMaterials => Set<RawMaterial>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<ProductionExperiment> ProductionExperiments => Set<ProductionExperiment>();
    public DbSet<ExperimentStep> ExperimentSteps => Set<ExperimentStep>();
    public DbSet<MarketStudy> MarketStudies => Set<MarketStudy>();
    public DbSet<Competitor> Competitors => Set<Competitor>();
    public DbSet<Risk> Risks => Set<Risk>();
    public DbSet<OptimizationRequest> OptimizationRequests => Set<OptimizationRequest>();
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ResourceCategory> ResourceCategories => Set<ResourceCategory>();
    public DbSet<AiConversation> AiConversations => Set<AiConversation>();
    public DbSet<AiMessage> AiMessages => Set<AiMessage>();
    public DbSet<AiMessageSource> AiMessageSources => Set<AiMessageSource>();
    public DbSet<AiAttachment> AiAttachments => Set<AiAttachment>();
    public DbSet<AiDocumentChunk> AiDocumentChunks => Set<AiDocumentChunk>();
    public DbSet<AiFeedback> AiFeedback => Set<AiFeedback>();
    public DbSet<AiToolInvocation> AiToolInvocations => Set<AiToolInvocation>();

    public DbSet<MarketAnalysisEntity> MarketAnalyses => Set<MarketAnalysisEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditTrail();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditTrail()
    {
        ChangeTracker.DetectChanges();
        var entries = ChangeTracker.Entries<AuditableEntity>()
            .Where(entry => entry.Entity is not AuditLog && entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToArray();
        if (entries.Length == 0) return;

        var now = (timeProvider ?? TimeProvider.System).GetUtcNow().UtcDateTime;
        var context = httpContextAccessor?.HttpContext;
        var actorId = Guid.TryParse(context?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedActor)
            ? parsedActor : (Guid?)null;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(AuditableEntity.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(AuditableEntity.CreatedBy)).CurrentValue = actorId;
            }
            if (entry.State != EntityState.Deleted)
            {
                entry.Property(nameof(AuditableEntity.UpdatedAt)).CurrentValue = now;
                entry.Property(nameof(AuditableEntity.UpdatedBy)).CurrentValue = actorId;
            }

            var entityType = entry.Metadata.ClrType.Name;
            var entityId = entry.Properties.FirstOrDefault(property => property.Metadata.IsPrimaryKey()
                && property.CurrentValue is Guid)?.CurrentValue as Guid?;
            var action = entry.State switch
            {
                EntityState.Added => "Create",
                EntityState.Modified => "Update",
                _ => "Delete"
            };
            AuditLogs.Add(AuditLog.Create(actorId, action, ModuleName(entry.Metadata.ClrType.Namespace),
                $"{action} operation on {entityType}.", AuditLevel.Information,
                context?.Connection.RemoteIpAddress?.ToString(), entityType, entityId,
                context?.TraceIdentifier, now));
        }
    }

    private static string ModuleName(string? typeNamespace) => typeNamespace?.Split('.').LastOrDefault() ?? "System";
}
