using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace FatturaFacile.Identity.Application.Security;

/// <summary>
/// Implements Argon2id password hashing meeting OWASP and Swiss nLPD specifications:
/// 64 MB memory limit (65536 KB), 4 iterations, 2 degrees of parallelism, 16-byte cryptographically secure salt.
/// </summary>
public sealed class Argon2idPasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int MemorySize = 65536; // 64 MB
    private const int Iterations = 4;
    private const int DegreeOfParallelism = 2;

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };

        var hash = argon2.GetBytes(KeySize);

        // Format: $argon2id$v=19$m=65536,t=4,p=2$<base64salt>$<base64hash>
        return $"$argon2id$v=19$m={MemorySize},t={Iterations},p={DegreeOfParallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) return false;

        var parts = passwordHash.Split('$');
        if (parts.Length != 6 || parts[1] != "argon2id") return false;

        var salt = Convert.FromBase64String(parts[4]);
        var expectedHash = Convert.FromBase64String(parts[5]);

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };

        var actualHash = argon2.GetBytes(expectedHash.Length);

        // Constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
