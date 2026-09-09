using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProductApp.Infrastructure.Persistence;

/// <summary>Provides a deterministic SQL Server context for EF Core design-time commands.</summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private const string DesignTimeConnection =
        "Server=localhost;Database=ProductApp;Trusted_Connection=True;TrustServerCertificate=True";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(DesignTimeConnection)
            .Options;
        return new ApplicationDbContext(options);
    }
}
