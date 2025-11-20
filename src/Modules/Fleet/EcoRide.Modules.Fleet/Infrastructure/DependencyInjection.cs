using EcoRide.BuildingBlocks.Application.Data;
using EcoRide.Modules.Fleet.Domain.Repositories;
using EcoRide.Modules.Fleet.Infrastructure.Persistence;
using EcoRide.Modules.Fleet.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcoRide.Modules.Fleet.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddFleetInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? dbContextOptions = null)
    {
        // Add DbContext
        if (dbContextOptions != null)
        {
            // Use custom database provider (for testing)
            services.AddDbContext<FleetDbContext>(dbContextOptions);
        }
        else
        {
            // Use PostgreSQL with PostGIS support for production
            services.AddDbContext<FleetDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "fleet");
                        npgsqlOptions.UseNetTopologySuite(); // Enable PostGIS support
                    });
            });
        }

        // Register IUnitOfWork
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FleetDbContext>());

        // Register repositories
        services.AddScoped<IVehicleRepository, VehicleRepository>();

        return services;
    }
}
