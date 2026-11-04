using System.Security.Cryptography;
using System.Text;

namespace FatturaFacile.Identity.Application.Security;

/// <summary>
/// Provides AES-256-GCM authenticated encryption and SHA-256 blind indexing for sensitive identifiers.
/// </summary>
public sealed class DataProtectorService : IDataProtectorService
{
    // 256-bit key derived or configured
    private readonly byte[] _key;

    public DataProtectorService(byte[]? key = null)
    {
        _key = key ?? SHA256.HashData(Encoding.UTF8.GetBytes("FatturaFacile-nLPD-Protection-Key-2026-SwissStandard"));
    }

    public string ComputeBlindIndex(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var normalized = input.Trim().ToUpperInvariant();
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        var nonce = RandomNumberGenerator.GetBytes(12); // 96-bit nonce for AES-GCM
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[16]; // 128-bit authentication tag

        using var aesGcm = new AesGcm(_key, 16);
        aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

        // Combine: nonce (12) + tag (16) + cipherText
        var result = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, nonce.Length + tag.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;

        var raw = Convert.FromBase64String(cipherText);
        var nonce = new byte[12];
        var tag = new byte[16];
        var cipherBytes = new byte[raw.Length - nonce.Length - tag.Length];

        Buffer.BlockCopy(raw, 0, nonce, 0, nonce.Length);
        Buffer.BlockCopy(raw, nonce.Length, tag, 0, tag.Length);
        Buffer.BlockCopy(raw, nonce.Length + tag.Length, cipherBytes, 0, cipherBytes.Length);

        var plainBytes = new byte[cipherBytes.Length];
        using var aesGcm = new AesGcm(_key, 16);
        aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
