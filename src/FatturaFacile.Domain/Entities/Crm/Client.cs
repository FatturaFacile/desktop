// <copyright file="Client.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Crm;

/// <summary>
/// Represents a customer or client organization/individual across Italian and Swiss fiscal regimes, maintaining billing metadata, electronic invoicing routing codes, contacts, and credit terms.
/// </summary>
public sealed class Client : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private readonly List<Contact> contacts = [];

    private Client()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization owning this client record.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the official registered legal corporate or personal name of the client.
    /// </summary>
    public string BusinessName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the optional commercial DBA (doing business as) trade name or commercial alias.
    /// </summary>
    public string? TradeName { get; }

    /// <summary>
    /// Gets the governing legal and fiscal jurisdiction (Italy, Switzerland, etc.) applicable to this client.
    /// </summary>
    public Jurisdiction Jurisdiction { get; private set; }

    /// <summary>
    /// Gets the Italian personal or corporate fiscal code (Codice Fiscale), required for Italian tax reporting.
    /// </summary>
    public FiscalCode? FiscalCode { get; private set; }

    /// <summary>
    /// Gets the Value Added Tax identification number (Partita IVA for Italy or MWST/TVA/IVA for Switzerland).
    /// </summary>
    public VatNumber? VatNumber { get; private set; }

    /// <summary>
    /// Gets the 7-character electronic invoicing recipient code (Codice Destinatario) for routing documents via the Italian Sistema di Interscambio (SDI), or "0000000" if delivered via PEC.
    /// </summary>
    public string? SdiCode { get; private set; }

    /// <summary>
    /// Gets the certified electronic mail address (Posta Elettronica Certificata - PEC) used for official electronic document transmission in Italy.
    /// </summary>
    public string? PecEmail { get; private set; }

    /// <summary>
    /// Gets the Swiss federal Business Identification Number (UID CHE-xxx.xxx.xxx), used in official Swiss commerce and tax registries.
    /// </summary>
    public string? SwissUid { get; private set; } // CHE-xxx.xxx.xxx

    /// <summary>
    /// Gets the Swiss QR-IBAN used for issuing standardized QR-bills (QR-Rechnung) with structured reference numbers.
    /// </summary>
    public QrIban? QrIban { get; private set; }

    /// <summary>
    /// Gets the Swiss canton two-letter abbreviation code (e.g., "TI", "GR", "ZH") indicating the fiscal cantonal jurisdiction.
    /// </summary>
    public string? CantonalCode { get; private set; }

    /// <summary>
    /// Gets the primary physical or registered office address of the client.
    /// </summary>
    public Address Address { get; private set; } = null!;

    /// <summary>
    /// Gets the primary operational contact email address for general business communication.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the primary telephone or landline contact number.
    /// </summary>
    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// Gets the agreed standard commercial invoice payment term in days from invoice date (e.g., 30, 60, or 90 days).
    /// </summary>
    public int PaymentTermDays { get; private set; } = 30;

    /// <summary>
    /// Gets the maximum approved credit exposure limit extended to this client.
    /// </summary>
    public Money CreditLimit { get; private set; } = Money.Euro(5000);

    /// <summary>
    /// Gets a value indicating whether this client account is active and permitted to receive new quotes, orders, or invoices.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Gets the collection of individual contact persons associated with this client entity.
    /// </summary>
    public IReadOnlyCollection<Contact> Contacts => contacts.AsReadOnly();

    /// <summary>
    /// Gets or sets the UTC timestamp when the client record was originally created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or process that created the client record.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the client record was last modified, or <see langword="null"/> if never modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or process that last modified the client record.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Factory method to create a new client profile under Italian fiscal jurisdiction, enforcing Italian electronic invoicing rules (SDI and PEC).
    /// </summary>
    /// <param name="tenantId">The unique identifier of the owning tenant.</param>
    /// <param name="businessName">The legal business name.</param>
    /// <param name="fiscalCode">The Italian fiscal code (Codice Fiscale).</param>
    /// <param name="vatNumber">The Italian VAT number (Partita IVA).</param>
    /// <param name="sdiCode">The 7-character SDI recipient code; if omitted or blank, defaults to "0000000".</param>
    /// <param name="pecEmail">The certified electronic mail (PEC) address for invoice delivery.</param>
    /// <param name="address">The registered billing and postal address.</param>
    /// <param name="email">The general commercial contact email.</param>
    /// <param name="phone">The optional contact telephone number.</param>
    /// <param name="paymentTermDays">The agreed payment terms in days (defaults to 30 days).</param>
    /// <returns>A fully configured <see cref="Client"/> instance for Italian billing.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="businessName"/> is null, empty, or consists only of white-space characters.</exception>
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
            Email = email?.Trim() ?? string.Empty,
            PhoneNumber = phone?.Trim(),
            PaymentTermDays = paymentTermDays,
            CreditLimit = Money.Euro(10000),
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Factory method to create a new client profile under Swiss fiscal jurisdiction, configured for Swiss QR-bill and cantonal tax compliance.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the owning tenant.</param>
    /// <param name="businessName">The legal registered company name.</param>
    /// <param name="swissUid">The Swiss enterprise identifier (UID CHE-xxx.xxx.xxx).</param>
    /// <param name="cantonalCode">The two-letter Swiss canton abbreviation (e.g., TI, GR, ZH).</param>
    /// <param name="qrIban">The optional Swiss QR-IBAN for invoice collections.</param>
    /// <param name="address">The Swiss billing and postal address.</param>
    /// <param name="email">The primary contact email address.</param>
    /// <param name="phone">The optional contact telephone number.</param>
    /// <param name="paymentTermDays">The agreed payment terms in days (defaults to 30 days).</param>
    /// <returns>A fully configured <see cref="Client"/> instance for Swiss billing.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="businessName"/> is null, empty, or consists only of white-space characters.</exception>
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
            Email = email?.Trim() ?? string.Empty,
            PhoneNumber = phone?.Trim() ?? string.Empty,
            PaymentTermDays = paymentTermDays,
            CreditLimit = Money.Chf(15000),
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Adds a contact person representation to this client's profile.
    /// </summary>
    /// <param name="firstName">The given name of the contact person.</param>
    /// <param name="lastName">The family name or surname.</param>
    /// <param name="email">The direct electronic mail address.</param>
    /// <param name="phone">The direct phone or mobile number.</param>
    /// <param name="role">The organizational role or department (e.g., "Accounts Payable", "Procurement Manager").</param>
    /// <param name="isPrimary">Specifies whether this contact is designated as the primary billing contact.</param>
    public void AddContact(
        string firstName,
        string lastName,
        string email,
        string? phone,
        string? role,
        bool isPrimary)
    {
        var contact = new Contact(
            Id,
            firstName,
            lastName,
            email,
            phone,
            role,
            isPrimary);
        contacts.Add(contact);
    }

    /// <summary>
    /// Updates the commercial credit limit and standard invoice payment terms, updating the modification timestamp.
    /// </summary>
    /// <param name="paymentTermDays">The updated standard payment window in days.</param>
    /// <param name="creditLimit">The updated maximum approved credit limit.</param>
    public void UpdateCommercialTerms(
        int paymentTermDays,
        Money creditLimit)
    {
        PaymentTermDays = paymentTermDays;
        CreditLimit = creditLimit;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the client account, preventing further transaction generation while preserving historical records.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
