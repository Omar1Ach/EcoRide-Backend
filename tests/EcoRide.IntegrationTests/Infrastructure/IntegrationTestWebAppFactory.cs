using EcoRide.Modules.Fleet.Infrastructure.Persistence;
using EcoRide.Modules.Security.Infrastructure.Persistence;
using EcoRide.Modules.Trip.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EcoRide.IntegrationTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for integration tests
/// Sets up in-memory database and test services
/// </summary>
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registrations
            services.RemoveAll(typeof(DbContextOptions<SecurityDbContext>));
            services.RemoveAll(typeof(DbContextOptions<FleetDbContext>));
            services.RemoveAll(typeof(DbContextOptions<TripDbContext>));

            // Add in-memory databases for testing
            services.AddDbContext<SecurityDbContext>(options =>
            {
                options.UseInMemoryDatabase("SecurityTestDb");
            });

            services.AddDbContext<FleetDbContext>(options =>
            {
                options.UseInMemoryDatabase("FleetTestDb");
            });

            services.AddDbContext<TripDbContext>(options =>
            {
                options.UseInMemoryDatabase("TripTestDb");
            });

            // Build service provider and ensure databases are created
            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;

            var securityDb = scopedServices.GetRequiredService<SecurityDbContext>();
            var fleetDb = scopedServices.GetRequiredService<FleetDbContext>();
            var tripDb = scopedServices.GetRequiredService<TripDbContext>();

            // Ensure databases are created
            securityDb.Database.EnsureCreated();
            fleetDb.Database.EnsureCreated();
            tripDb.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }
}
