using EcoRide.BuildingBlocks.Application.Data;
using EcoRide.Modules.Trip.Domain.Repositories;
using EcoRide.Modules.Trip.Infrastructure.Persistence;
using EcoRide.Modules.Trip.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcoRide.Modules.Trip.Infrastructure;

/// <summary>
/// Dependency injection configuration for Trip module
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddTripInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? dbContextOptions = null)
    {
        // Add DbContext
        if (dbContextOptions != null)
        {
            // Use custom database provider (for testing)
            services.AddDbContext<TripDbContext>(dbContextOptions);
        }
        else
        {
            // Use PostgreSQL for production
            services.AddDbContext<TripDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "trip");
                    });
            });
        }

        // Repositories
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IActiveTripRepository, ActiveTripRepository>();
        services.AddScoped<IReceiptRepository, ReceiptRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TripDbContext>());

        return services;
    }
}
