using ProductApp.Infrastructure.Persistence;

namespace ProductApp.Infrastructure.Tests.Persistence;

public sealed class SqlServerProviderTests
{
    [Fact]
    public void DesignTimeFactory_UsesSqlServerProvider()
    {
        using var context = new ApplicationDbContextFactory().CreateDbContext([]);

        Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", context.Database.ProviderName);
    }
}
