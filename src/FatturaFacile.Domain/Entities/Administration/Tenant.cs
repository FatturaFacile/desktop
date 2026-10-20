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