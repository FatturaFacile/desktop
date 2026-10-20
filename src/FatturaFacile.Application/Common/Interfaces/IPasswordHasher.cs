namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Defines the contract for password hasher.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Create Hash.
    /// </summary>
    /// <param name="password">The password.</param>
    /// <param name="hash">The hash.</param>
    /// <param name="salt">The salt.</param>
    public void CreateHash(string password, out string hash, out string salt);

/// <summary>
    /// Verify Hash.
    /// </summary>
    /// <param name="password">The password.</param>
    /// <param name="hash">The hash.</param>
    /// <param name="salt">The salt.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public bool VerifyHash(string password, string hash, string salt);
}
