using FatturaFacile.Domain.Shared.Primitives;

namespace FatturaFacile.Crm.Domain.Aggregates;

public enum ContactType
{
    Customer = 1,
    Supplier = 2,
    Lead = 3
}

public sealed class Contact : AggregateRoot<Guid>, IHasTenant, ISoftDeletable, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? TradeName { get; private set; }
    public ContactType ContactType { get; private set; }
    public string? VatNumber { get; private set; }
    public string? TaxCode { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Pec { get; private set; }
    public string? SdiCode { get; private set; }
    public string CountryCode { get; private set; } = "IT";

    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    // Audit
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private Contact() { }

    public static Contact Create(
        Guid tenantId,
        string name,
        ContactType type,
        string countryCode = "IT",
        string? vatNumber = null,
        string? taxCode = null,
        string? email = null,
        string? sdiCode = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Contact name is required.", nameof(name));

        return new Contact
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name.Trim(),
            ContactType = type,
            CountryCode = countryCode.ToUpperInvariant(),
            VatNumber = vatNumber?.Trim(),
            TaxCode = taxCode?.Trim(),
            Email = email?.Trim(),
            SdiCode = sdiCode?.Trim() ?? (countryCode == "IT" ? "0000000" : null),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
