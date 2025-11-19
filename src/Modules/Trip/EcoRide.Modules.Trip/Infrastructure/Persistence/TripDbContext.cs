using EcoRide.BuildingBlocks.Application.Data;
using EcoRide.Modules.Trip.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace EcoRide.Modules.Trip.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for Trip module (reservations and trips)
/// </summary>
public sealed class TripDbContext : DbContext, IUnitOfWork
{
    public TripDbContext(DbContextOptions<TripDbContext> options) : base(options)
    {
    }

    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ActiveTrip> Trips => Set<ActiveTrip>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("trip");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TripDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
