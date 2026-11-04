using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FatturaFacile.Identity.Domain.Aggregates;
using FatturaFacile.Identity.Domain.Entities;

namespace FatturaFacile.Identity.Infrastructure.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Name).HasMaxLength(150).IsRequired();
        builder.Property(t => t.Slug).HasMaxLength(50).IsRequired();
        builder.Property(t => t.IsActive).IsRequired();

        builder.HasIndex(t => t.Slug).IsUnique();
    }
}

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.NormalizedEmail).HasMaxLength(128).IsRequired();
        builder.Property(u => u.EncryptedEmail).HasMaxLength(500).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(u => u.SecurityStamp).HasMaxLength(50).IsRequired();
        builder.Property(u => u.PreferredLanguage).HasMaxLength(10).IsRequired();
        builder.Property(u => u.IsActive).IsRequired();

        builder.HasIndex(u => new { u.TenantId, u.NormalizedEmail }).IsUnique();
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(r => r.ReplacedByTokenHash).HasMaxLength(128);

        builder.HasIndex(r => r.TokenHash).IsUnique();
        builder.HasIndex(r => r.UserId);
    }
}

public sealed class SecurityAuditLogConfiguration : IEntityTypeConfiguration<SecurityAuditLog>
{
    public void Configure(EntityTypeBuilder<SecurityAuditLog> builder)
    {
        builder.ToTable("security_audit_logs");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.EventType).HasMaxLength(100).IsRequired();
        builder.Property(s => s.IpAddress).HasMaxLength(50);
        builder.Property(s => s.UserAgent).HasMaxLength(255);

        builder.HasIndex(s => new { s.TenantId, s.TimestampUtc });
    }
}
