using EcoRide.BuildingBlocks.Application.Data;
using EcoRide.Modules.Security.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace EcoRide.Modules.Security.Infrastructure.Persistence;

/// <summary>
/// DbContext for the Security module
/// </summary>
public sealed class SecurityDbContext : DbContext, IUnitOfWork
{
    public SecurityDbContext(DbContextOptions<SecurityDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("security");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SecurityDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
