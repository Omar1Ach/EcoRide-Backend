using EcoRide.Modules.Security.Domain.Aggregates;
using EcoRide.Modules.Security.Domain.Repositories;
using EcoRide.Modules.Security.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EcoRide.Modules.Security.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for OtpCode aggregate
/// </summary>
public sealed class OtpRepository : IOtpRepository
{
    private readonly SecurityDbContext _context;

    public OtpRepository(SecurityDbContext context)
    {
        _context = context;
    }

    public async Task<OtpCode?> GetLatestByPhoneAsync(PhoneNumber phone, CancellationToken cancellationToken = default)
    {
        return await _context.OtpCodes
            .Where(o => o.PhoneNumber == phone && !o.Verified)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CountRecentOtpRequestsAsync(
        PhoneNumber phone,
        TimeSpan within,
        CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(within);

        return await _context.OtpCodes
            .Where(o => o.PhoneNumber == phone && o.CreatedAt >= cutoffTime)
            .CountAsync(cancellationToken);
    }

    public async Task AddAsync(OtpCode otpCode, CancellationToken cancellationToken = default)
    {
        await _context.OtpCodes.AddAsync(otpCode, cancellationToken);
    }

    public void Update(OtpCode otpCode)
    {
        _context.OtpCodes.Update(otpCode);
    }
}
