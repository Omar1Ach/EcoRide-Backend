using EcoRide.Modules.Security.Application.Services;

namespace EcoRide.Modules.Security.Infrastructure.Services;

/// <summary>
/// BCrypt-based password hasher implementation
/// </summary>
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
