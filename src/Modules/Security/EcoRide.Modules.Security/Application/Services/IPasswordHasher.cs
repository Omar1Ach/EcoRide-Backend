namespace EcoRide.Modules.Security.Application.Services;

/// <summary>
/// Service for hashing and verifying passwords using BCrypt
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password using BCrypt with cost factor 12
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Verifies a password against a BCrypt hash
    /// </summary>
    bool Verify(string password, string hash);
}
