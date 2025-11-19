using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EcoRide.Modules.Security.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for SecurityDbContext to enable EF Core migrations
/// </summary>
public class DesignTimeSecurityDbContextFactory : IDesignTimeDbContextFactory<SecurityDbContext>
{
    public SecurityDbContext CreateDbContext(string[] args)
    {
        // Use hardcoded connection string for design-time migrations
        var connectionString = "Host=localhost;Port=5432;Database=EcoRide;Username=postgres;Password=Omar123";

        // Create DbContextOptions
        var optionsBuilder = new DbContextOptionsBuilder<SecurityDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "security"));

        return new SecurityDbContext(optionsBuilder.Options);
    }
}
