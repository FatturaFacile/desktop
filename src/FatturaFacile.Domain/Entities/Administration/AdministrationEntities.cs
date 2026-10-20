using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Administration;

public sealed class Tenant : AggregateRoot<Guid>, IAuditableEntity
{
    /// <summary>
    /// e.g. &quot;TENANT-IT-01&quot;, &quot;TENANT-CH-01&quot;
    /// </summary>
    public string Code { get; private set; } = string.Empty;
    public string BusinessName { get; private set; } = string.Empty;
    /// <summary>
    /// &quot;IT&quot; or &quot;CH&quot;
    /// </summary>
    public string CountryCode { get; private set; } = "IT";
    public Currency BaseCurrency { get; private set; } = Currency.EUR;
    public string? VatNumber { get; private set; }
    public string? FiscalCode { get; private set; }
    public string? SdiCode { get; private set; }
    public string? SwissUid { get; private set; }
    public string? QrIban { get; private set; }
    public string ContactEmail { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private readonly List<Branch> _branches = [];
    public IReadOnlyCollection<Branch> Branches => _branches.AsReadOnly();

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private Tenant()
    {
    }

    public static Tenant Create(
        string code,
        string businessName,
        string countryCode,
        Currency baseCurrency,
        string contactEmail,
        string? vatNumber = null,
        string? fiscalCode = null,
        string? sdiCode = null,
        string? swissUid = null,
        string? qrIban = null)
    {
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            BusinessName = businessName.Trim(),
            CountryCode = countryCode.Trim().ToUpperInvariant(),
            BaseCurrency = baseCurrency,
            ContactEmail = contactEmail.Trim(),
            VatNumber = vatNumber?.Trim(),
            FiscalCode = fiscalCode?.Trim(),
            SdiCode = sdiCode?.Trim(),
            SwissUid = swissUid?.Trim(),
            QrIban = qrIban?.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void AddBranch(string code, string name, Address address, bool isHeadquarters = false) => _branches.Add(new Branch(Id, code, name, address, isHeadquarters));
}

public sealed class Branch : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string BranchCode { get; } = string.Empty;
    public string Name { get; } = string.Empty;
    public Address Address { get; } = null!;
    public bool IsHeadquarters { get; }

    internal Branch(Guid tenantId, string code, string name, Address address, bool isHq)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        BranchCode = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Address = address;
        IsHeadquarters = isHq;
    }

    private Branch()
    {
    }
}

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

public sealed class ExchangeRate : Entity<Guid>
{
    public Currency BaseCurrency { get; private set; }
    public Currency TargetCurrency { get; private set; }
    public decimal Rate { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    /// <summary>
    /// ECB (European Central Bank) or SNB (Swiss National Bank)
    /// </summary>
    public string Source { get; private set; } = "ECB";

    private ExchangeRate()
    {
    }

    public static ExchangeRate Create(Currency baseCurr, Currency targetCurr, decimal rate, DateTime date, string source = "ECB")
    {
        return new ExchangeRate
        {
            Id = Guid.NewGuid(),
            BaseCurrency = baseCurr,
            TargetCurrency = targetCurr,
            Rate = rate,
            EffectiveDate = date,
            Source = source,
        };
    }
}

public sealed class AppTranslation : Entity<Guid>
{
    /// <summary>
    /// Null for system defaults
    /// </summary>
    public Guid? TenantId { get; private set; }
    /// <summary>
    /// &quot;it&quot;, &quot;de&quot;, &quot;fr&quot;, &quot;en&quot;
    /// </summary>
    public string Locale { get; private set; } = "it";
    public string Key { get; private set; } = string.Empty;
    public string DefaultValue { get; private set; } = string.Empty;
    public string? CustomValue { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public string EffectiveValue => !string.IsNullOrWhiteSpace(CustomValue) ? CustomValue : DefaultValue;

    private AppTranslation()
    {
    }

    public static AppTranslation Create(string locale, string key, string defaultValue, string? customValue = null, Guid? tenantId = null)
    {
        return new AppTranslation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Locale = locale.ToLowerInvariant(),
            Key = key.Trim(),
            DefaultValue = defaultValue,
            CustomValue = customValue,
            UpdatedAtUtc = DateTime.UtcNow,
        };
    }

    public void UpdateCustomValue(string? customValue)
    {
        CustomValue = customValue;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
