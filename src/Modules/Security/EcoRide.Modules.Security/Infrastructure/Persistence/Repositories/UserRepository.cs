using EcoRide.Modules.Security.Domain.Aggregates;
using EcoRide.Modules.Security.Domain.Repositories;
using EcoRide.Modules.Security.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EcoRide.Modules.Security.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for User aggregate
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly SecurityDbContext _context;

    public UserRepository(SecurityDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.DeletedAt == null)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.DeletedAt == null)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByPhoneAsync(PhoneNumber phone, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.DeletedAt == null)
            .FirstOrDefaultAsync(u => u.PhoneNumber == phone, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Email email, PhoneNumber phone, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.DeletedAt == null)
            .AnyAsync(u => u.Email == email || u.PhoneNumber == phone, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }
}
