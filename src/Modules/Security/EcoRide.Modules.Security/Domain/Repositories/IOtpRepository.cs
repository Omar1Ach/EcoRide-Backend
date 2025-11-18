using EcoRide.Modules.Security.Domain.Aggregates;
using EcoRide.Modules.Security.Domain.ValueObjects;

namespace EcoRide.Modules.Security.Domain.Repositories;

/// <summary>
/// Repository interface for OtpCode aggregate
/// </summary>
public interface IOtpRepository
{
    Task<OtpCode?> GetLatestByPhoneAsync(PhoneNumber phone, CancellationToken cancellationToken = default);
    Task<int> CountRecentOtpRequestsAsync(PhoneNumber phone, TimeSpan within, CancellationToken cancellationToken = default);
    Task AddAsync(OtpCode otpCode, CancellationToken cancellationToken = default);
    void Update(OtpCode otpCode);
}
