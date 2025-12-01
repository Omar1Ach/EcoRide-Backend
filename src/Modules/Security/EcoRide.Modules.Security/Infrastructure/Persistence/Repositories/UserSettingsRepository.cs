using EcoRide.Modules.Security.Domain.Entities;
using EcoRide.Modules.Security.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRide.Modules.Security.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for UserSettings entity
/// </summary>
public sealed class UserSettingsRepository : IUserSettingsRepository
{
    private readonly SecurityDbContext _context;

    public UserSettingsRepository(SecurityDbContext context)
    {
        _context = context;
    }

    public async Task<UserSettings?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(UserSettings settings, CancellationToken cancellationToken = default)
    {
        await _context.UserSettings.AddAsync(settings, cancellationToken);
    }

    public void Update(UserSettings settings)
    {
        _context.UserSettings.Update(settings);
    }
}
