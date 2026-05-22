using BC = BCrypt.Net.BCrypt;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Authentication;

/// <summary>
/// Service for hashing and verifying passwords using BCrypt
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        // Work factor 10 provides ~250-300ms hashing (down from ~800-1000ms at factor 12)
        // Still cryptographically secure while improving performance
        return BC.HashPassword(password, workFactor: 10);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BC.Verify(password, hash);
    }
}
