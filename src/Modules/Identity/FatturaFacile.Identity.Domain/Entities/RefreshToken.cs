using FatturaFacile.Domain.Shared.Primitives;

namespace FatturaFacile.Identity.Domain.Entities;

/// <summary>
/// Represents a cryptographically secure refresh token for session continuity and rotation.
/// </summary>
public sealed class RefreshToken : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsActive => RevokedAtUtc == null && !IsExpired;
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAtUtc;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string tokenHash, TimeSpan lifetime)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = DateTimeOffset.UtcNow.Add(lifetime)
        };
    }

    public void Revoke(string? replacedByTokenHash = null)
    {
        RevokedAtUtc = DateTimeOffset.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
