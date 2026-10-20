using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Crm;

public sealed class Client : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string BusinessName { get; private set; } = string.Empty;
    public string? TradeName { get; }
    public Jurisdiction Jurisdiction { get; private set; }

    /// <summary>
    /// Italian Fiscal Details
    /// </summary>
    public FiscalCode? FiscalCode { get; private set; }
    public VatNumber? VatNumber { get; private set; }

    /// <summary>
    /// 7-character SDI recipient code or 0000000
    /// </summary>
    public string? SdiCode { get; private set; }
    public string? PecEmail { get; private set; }

    /// <summary>
    /// Swiss Fiscal Details
    /// </summary>
    public string? SwissUid { get; private set; } // CHE-xxx.xxx.xxx
    public QrIban? QrIban { get; private set; }

    /// <summary>
    /// TI, GR, ZH, etc.
    /// </summary>
    public string? CantonalCode { get; private set; }

    /// <summary>
    /// Contact & Commercial
    /// </summary>
    public Address Address { get; private set; } = null!;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public int PaymentTermDays { get; private set; } = 30;
    public Money CreditLimit { get; private set; } = Money.Euro(5000);
    public bool IsActive { get; private set; } = true;

    private readonly List<Contact> _contacts = [];
    public IReadOnlyCollection<Contact> Contacts => _contacts.AsReadOnly();

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private Client()
    {
    }

    public static Client CreateItalian(
        Guid tenantId,
        string businessName,
        FiscalCode fiscalCode,
        VatNumber vatNumber,
        string sdiCode,
        string pecEmail,
        Address address,
        string email,
        string? phone = null,
        int paymentTermDays = 30)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(businessName);
        return new Client
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BusinessName = businessName.Trim(),
            Jurisdiction = Jurisdiction.Italy,
            FiscalCode = fiscalCode,
            VatNumber = vatNumber,
            SdiCode = string.IsNullOrWhiteSpace(sdiCode) ? "0000000" : sdiCode.Trim().ToUpperInvariant(),
            PecEmail = pecEmail?.Trim(),
            Address = address,
            Email = email.Trim(),
            PhoneNumber = phone?.Trim(),
            PaymentTermDays = paymentTermDays,
            CreditLimit = Money.Euro(10000),
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public static Client CreateSwiss(
        Guid tenantId,
        string businessName,
        string swissUid,
        string cantonalCode,
        QrIban? qrIban,
        Address address,
        string email,
        string? phone = null,
        int paymentTermDays = 30)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(businessName);
        return new Client
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BusinessName = businessName.Trim(),
            Jurisdiction = Jurisdiction.Switzerland,
            SwissUid = swissUid?.Trim().ToUpperInvariant(),
            CantonalCode = cantonalCode?.Trim().ToUpperInvariant(),
            QrIban = qrIban,
            Address = address,
            Email = email.Trim(),
            PhoneNumber = phone?.Trim(),
            PaymentTermDays = paymentTermDays,
            CreditLimit = Money.Chf(15000),
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void AddContact(string firstName, string lastName, string email, string? phone, string? role, bool isPrimary)
    {
        var contact = new Contact(Id, firstName, lastName, email, phone, role, isPrimary);
        _contacts.Add(contact);
    }

    public void UpdateCommercialTerms(int paymentTermDays, Money creditLimit)
    {
        PaymentTermDays = paymentTermDays;
        CreditLimit = creditLimit;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}