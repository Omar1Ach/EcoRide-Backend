using EcoRide.Modules.Security.Application.Data;
using EcoRide.Modules.Security.Domain.Aggregates;
using EcoRide.Modules.Security.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoRide.Modules.Security.Infrastructure.Persistence;

/// <summary>
/// DbContext for the Security module
/// Implements ISecurityUnitOfWork for transaction management
/// </summary>
public sealed class SecurityDbContext : DbContext, ISecurityUnitOfWork
{
    public SecurityDbContext(DbContextOptions<SecurityDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<PaymentMethodEntity> PaymentMethods => Set<PaymentMethodEntity>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("security");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SecurityDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries().ToList();
        Console.WriteLine($"[SecurityDbContext] SaveChangesAsync called with {entries.Count} entries");

        foreach (var entry in entries)
        {
            Console.WriteLine($"  - {entry.Entity.GetType().Name}: {entry.State}");
        }

        var result = await base.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[SecurityDbContext] Saved {result} changes to database");

        return result;
    }
}
