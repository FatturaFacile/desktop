using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Administration;

public sealed class User : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string PasswordSalt { get; private set; } = string.Empty;
    public UserRole Role { get; private set; } = UserRole.Admin;
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginUtc { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private User()
    {
    }

    public static User Create(
        Guid tenantId,
        string username,
        string email,
        string fullName,
        string passwordHash,
        string salt,
        UserRole role)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Username = username.Trim().ToLowerInvariant(),
            Email = email.Trim().ToLowerInvariant(),
            FullName = fullName.Trim(),
            PasswordHash = passwordHash,
            PasswordSalt = salt,
            Role = role,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void RecordLogin() => LastLoginUtc = DateTime.UtcNow;

    public void ChangePassword(string newHash, string newSalt)
    {
        PasswordHash = newHash;
        PasswordSalt = newSalt;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}