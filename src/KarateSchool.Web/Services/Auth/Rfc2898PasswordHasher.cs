using System.Security.Cryptography;

namespace KarateSchool.Web.Services.Auth;

/// <summary>
/// Password hasher using PBKDF2 (Rfc2898DeriveBytes / HMAC-SHA256). Passwords are never stored
/// or transmitted in plain text; only the salted hash is persisted.
/// </summary>
public class Rfc2898PasswordHasher : IPasswordHasher
{
    private const int SaltSizeBytes = 16;
    private const int KeySizeBytes = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string HashPassword(string plainTextPassword)
    {
        if (string.IsNullOrEmpty(plainTextPassword))
            throw new ArgumentException("Password cannot be empty.", nameof(plainTextPassword));

        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var key = Rfc2898DeriveBytes.Pbkdf2(plainTextPassword, salt, Iterations, Algorithm, KeySizeBytes);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public bool VerifyPassword(string plainTextPassword, string passwordHash)
    {
        if (string.IsNullOrEmpty(plainTextPassword) || string.IsNullOrEmpty(passwordHash))
            return false;

        var parts = passwordHash.Split('.', 3);
        if (parts.Length != 3)
            return false;
        if (!int.TryParse(parts[0], out var iterations))
            return false;

        byte[] salt, expectedKey;
        try
        {
            salt = Convert.FromBase64String(parts[1]);
            expectedKey = Convert.FromBase64String(parts[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualKey = Rfc2898DeriveBytes.Pbkdf2(plainTextPassword, salt, iterations, Algorithm, expectedKey.Length);
        return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
    }
}
