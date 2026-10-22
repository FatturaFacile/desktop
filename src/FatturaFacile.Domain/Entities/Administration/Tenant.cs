// <copyright file="Tenant.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Administration;

/// <summary>
/// Represents a legal business entity or enterprise tenant in the multi-tenant architecture, managing organization-level fiscal identifiers, branches, and currency preferences across Italian and Swiss jurisdictions.
/// </summary>
public sealed class Tenant : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<Branch> branches = [];

    private Tenant()
    {
    }

    /// <summary>
    /// Gets the unique uppercase tenant code (e.g., &quot;TENANT-IT-01&quot;, &quot;TENANT-CH-01&quot;).
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the legal registered corporate or commercial business name.
    /// </summary>
    public string BusinessName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the primary ISO 3166-1 alpha-2 country code governing legal incorporation and default fiscal rules (&quot;IT&quot; or &quot;CH&quot;).
    /// </summary>
    public string CountryCode { get; private set; } = "IT";

    /// <summary>
    /// Gets the default functional base accounting currency used for ledger balances and financial reporting.
    /// </summary>
    public Currency BaseCurrency { get; private set; } = Currency.EUR;

    /// <summary>
    /// Gets the optional registered Value Added Tax identification number (Partita IVA for Italy or MWST/TVA/IVA for Switzerland).
    /// </summary>
    public string? VatNumber { get; private set; }

    /// <summary>
    /// Gets the Italian fiscal code (Codice Fiscale) for corporate tax filings and official communications, if applicable.
    /// </summary>
    public string? FiscalCode { get; private set; }

    /// <summary>
    /// Gets the 7-character electronic invoice recipient code (Codice Destinatario) for routing via the Italian Sistema di Interscambio (SDI), or "0000000" if delivered via PEC.
    /// </summary>
    public string? SdiCode { get; private set; }

    /// <summary>
    /// Gets the Swiss corporate enterprise identification number (Unternehmens-Identifikationsnummer / UID, e.g., CHE-123.456.789), if applicable.
    /// </summary>
    public string? SwissUid { get; private set; }

    /// <summary>
    /// Gets the Swiss QR-IBAN used for issuing standardized QR-bills (QR-Rechnung) with structured reference numbers.
    /// </summary>
    public string? QrIban { get; private set; }

    /// <summary>
    /// Gets the primary operational contact email address for corporate and administrative correspondence.
    /// </summary>
    public string ContactEmail { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this tenant account is currently active and permitted to process transactions.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Gets the collection of registered operational branches and office locations associated with this tenant.
    /// </summary>
    public IReadOnlyCollection<Branch> Branches => branches.AsReadOnly();

    /// <summary>
    /// Gets or sets the UTC timestamp when the tenant record was initially created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity or username of the user who provisioned the tenant record.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the tenant record was last modified, or <see langword="null"/> if never modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity or username of the user who performed the last modification.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates and initializes a new <see cref="Tenant"/> aggregate root instance.
    /// </summary>
    /// <param name="code">The unique organizational code for this tenant.</param>
    /// <param name="businessName">The official legal business name.</param>
    /// <param name="countryCode">The primary country of operation (&quot;IT&quot; or &quot;CH&quot;).</param>
    /// <param name="baseCurrency">The baseline operating currency for accounting ledgers.</param>
    /// <param name="contactEmail">The main contact email address.</param>
    /// <param name="vatNumber">The VAT registration number (e.g., Italian Partita IVA or Swiss TVA number).</param>
    /// <param name="fiscalCode">The Italian Codice Fiscale, if applicable.</param>
    /// <param name="sdiCode">The 7-character Italian SDI routing code.</param>
    /// <param name="swissUid">The Swiss enterprise identifier (UID CHE-xxx.xxx.xxx).</param>
    /// <param name="qrIban">The Swiss QR-IBAN for payment collection.</param>
    /// <returns>A fully configured and active <see cref="Tenant"/> aggregate root.</returns>
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
            Code = code?.Trim().ToUpperInvariant() ?? string.Empty,
            BusinessName = businessName?.Trim() ?? string.Empty,
            CountryCode = countryCode?.Trim().ToUpperInvariant() ?? string.Empty,
            BaseCurrency = baseCurrency,
            ContactEmail = contactEmail?.Trim() ?? string.Empty,
            VatNumber = vatNumber?.Trim() ?? string.Empty,
            FiscalCode = fiscalCode?.Trim() ?? string.Empty,
            SdiCode = sdiCode?.Trim() ?? string.Empty,
            SwissUid = swissUid?.Trim() ?? string.Empty,
            QrIban = qrIban?.Trim() ?? string.Empty,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Adds a new operational office or warehouse branch to this tenant's organization profile.
    /// </summary>
    /// <param name="code">The unique organizational branch code.</param>
    /// <param name="name">The descriptive display name of the branch office.</param>
    /// <param name="address">The physical postal address of the branch.</param>
    /// <param name="isHeadquarters">Specifies whether this branch serves as the corporate headquarters.</param>
    public void AddBranch(
        string code,
        string name,
        Address address,
        bool isHeadquarters = false) =>
        branches.Add(
            new Branch(
                Id,
                code ?? string.Empty,
                name ?? string.Empty,
                address,
                isHeadquarters));
}
