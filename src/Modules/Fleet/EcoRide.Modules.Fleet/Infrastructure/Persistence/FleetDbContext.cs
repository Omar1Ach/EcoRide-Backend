using EcoRide.BuildingBlocks.Application.Data;
using EcoRide.Modules.Fleet.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace EcoRide.Modules.Fleet.Infrastructure.Persistence;

/// <summary>
/// DbContext for the Fleet module
/// </summary>
public sealed class FleetDbContext : DbContext, IUnitOfWork
{
    public FleetDbContext(DbContextOptions<FleetDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("fleet");

        // Enable PostGIS extension
        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FleetDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
