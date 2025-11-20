using EcoRide.Modules.Fleet.Infrastructure;
using EcoRide.Modules.Fleet.Infrastructure.Persistence;
using EcoRide.Modules.Security.Infrastructure;
using EcoRide.Modules.Security.Infrastructure.Persistence;
using EcoRide.Modules.Trip.Infrastructure;
using EcoRide.Modules.Trip.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace EcoRide.IntegrationTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for integration tests
/// Uses Testcontainers with PostgreSQL+PostGIS for realistic database testing
/// Provides isolated, reproducible test environment with full geospatial support
/// </summary>
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgis/postgis:16-3.4")
        .WithDatabase("ecoride_test")
        .WithUsername("test")
        .WithPassword("test")
        .WithCleanUp(true)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to Testing - this prevents Program.cs from registering infrastructure
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override connection string with TestContainers connection string
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgresContainer.GetConnectionString()
            });
        });

        builder.ConfigureServices(services =>
        {
            // Register infrastructure with PostgreSQL TestContainer
            services.AddSecurityInfrastructure(
                null!, // Configuration will be injected via ConfigureAppConfiguration
                options => options.UseNpgsql(
                    _postgresContainer.GetConnectionString(),
                    npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "security")));

            services.AddFleetInfrastructure(
                null!, // Configuration will be injected via ConfigureAppConfiguration
                options => options.UseNpgsql(
                    _postgresContainer.GetConnectionString(),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "fleet");
                        npgsqlOptions.UseNetTopologySuite(); // Enable PostGIS support
                    }));

            services.AddTripInfrastructure(
                null!, // Configuration will be injected via ConfigureAppConfiguration
                options => options.UseNpgsql(
                    _postgresContainer.GetConnectionString(),
                    npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "trip")));
        });
    }

    public async Task InitializeAsync()
    {
        // Start PostgreSQL container
        await _postgresContainer.StartAsync();

        // Ensure databases are created with schema
        using var scope = Services.CreateScope();

        var securityDb = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
        var fleetDb = scope.ServiceProvider.GetRequiredService<FleetDbContext>();
        var tripDb = scope.ServiceProvider.GetRequiredService<TripDbContext>();

        // Run migrations to create schemas and tables
        await securityDb.Database.MigrateAsync();
        await fleetDb.Database.MigrateAsync();
        await tripDb.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}
