namespace FatturaFacile.Identity.Application.Security;

/// <summary>
/// Password hashing contract supporting Argon2id.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes the raw password string using Argon2id with cryptographically secure salt.
    /// </summary>
    /// <param name="password">The plaintext password.</param>
    /// <returns>The encoded hash string containing algorithm parameters, salt, and key.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies whether the provided plaintext password matches the stored Argon2id hash.
    /// </summary>
    /// <param name="password">The plaintext password to verify.</param>
    /// <param name="passwordHash">The stored Argon2id hash.</param>
    /// <returns>True if password matches; otherwise false.</returns>
    bool VerifyPassword(string password, string passwordHash);
}

/// <summary>
/// Data encryption and blind-indexing helper for Swiss nLPD / FADP compliance.
/// </summary>
public interface IDataProtectorService
{
    /// <summary>
    /// Computes a deterministic SHA-256 blind index hash for indexed email search.
    /// </summary>
    string ComputeBlindIndex(string input);

    /// <summary>
    /// Encrypts sensitive personal data using AES-256-GCM.
    /// </summary>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts AES-256-GCM cipher text.
    /// </summary>
    string Decrypt(string cipherText);
}
