using EcoRide.Modules.Security.Domain.Entities;

namespace EcoRide.Modules.Security.Domain.Repositories;

/// <summary>
/// Repository interface for UserSettings entity
/// </summary>
public interface IUserSettingsRepository
{
    Task<UserSettings?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserSettings settings, CancellationToken cancellationToken = default);
    void Update(UserSettings settings);
}
