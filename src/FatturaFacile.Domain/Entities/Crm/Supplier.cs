// <copyright file="Supplier.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Crm;

/// <summary>
/// Represents a vendor or supplier organization across Italian and Swiss fiscal regimes, maintaining supplier tax credentials, banking details, and procurement terms.
/// </summary>
public sealed class Supplier : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="Supplier"/> class for entity framework or serialization.
    /// </summary>
    private Supplier()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization owning this supplier record.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the legal or commercial registered business name of the supplier.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the governing legal and tax jurisdiction of the vendor (e.g., Italy, Switzerland).
    /// </summary>
    public Jurisdiction Jurisdiction { get; private set; }

    /// <summary>
    /// Gets the European or Italian Value Added Tax identifier (Partita IVA), formatted with its country prefix.
    /// </summary>
    public VatNumber? VatNumber { get; private set; }

    /// <summary>
    /// Gets the Swiss corporate enterprise identification number (UID CHE-xxx.xxx.xxx) registered with the Swiss Federal Statistical Office.
    /// </summary>
    public string? SwissUid { get; private set; }

    /// <summary>
    /// Gets the 7-character electronic invoice recipient code (Codice Destinatario) or PEC address used by Italian vendors for Sistema di Interscambio (SDI) routing.
    /// </summary>
    public string? SdiCode { get; private set; }

    /// <summary>
    /// Gets the physical business location or postal headquarters address of the supplier.
    /// </summary>
    public Address Address { get; private set; } = null!;

    /// <summary>
    /// Gets the normalized International Bank Account Number (IBAN) designated for accounts payable disbursements.
    /// </summary>
    public string? Iban { get; private set; }

    /// <summary>
    /// Gets the Bank Identifier Code (BIC / SWIFT) identifying the vendor's financial institution for international wire transfers.
    /// </summary>
    public string? BicSwift { get; private set; }

    /// <summary>
    /// Gets the agreed standard accounts payable payment terms duration in days from invoice reception.
    /// </summary>
    public int PaymentTermsDays { get; private set; } = 30;

    /// <summary>
    /// Gets additional operational notes, vendor contracts, or procurement terms associated with this supplier.
    /// </summary>
    public string? Notes { get; }

    /// <summary>
    /// Gets a value indicating whether this supplier profile is currently active for issuing purchase orders and recording vendor bills.
    /// </summary>
    public bool IsActive { get; } = true;

    /// <summary>
    /// Gets or sets the UTC timestamp when this supplier record was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or process that registered this supplier.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this supplier record was last updated, or <see langword="null"/> if never modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or process that last modified this supplier record.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates and initializes a new <see cref="Supplier"/> aggregate root instance.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the owning tenant organization.</param>
    /// <param name="name">The legal or commercial business name of the vendor.</param>
    /// <param name="jurisdiction">The primary fiscal jurisdiction of the vendor (Italy or Switzerland).</param>
    /// <param name="address">The physical or postal address of the vendor.</param>
    /// <param name="vatNumber">The optional VAT registration number (Partita IVA or TVA/MWST).</param>
    /// <param name="swissUid">The optional Swiss Business Identification Number (UID CHE-xxx.xxx.xxx).</param>
    /// <param name="sdiCode">The optional Italian SDI electronic routing code or PEC address.</param>
    /// <param name="iban">The vendor bank account IBAN for invoice payments.</param>
    /// <param name="bicSwift">The SWIFT/BIC code for bank routing.</param>
    /// <param name="paymentTermsDays">The standard agreed payment terms in days (defaults to 30 days).</param>
    /// <returns>A fully configured <see cref="Supplier"/> entity.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null, empty, or consists only of white-space characters.</exception>
    public static Supplier Create(
        Guid         tenantId,
        string       name,
        Jurisdiction jurisdiction,
        Address      address,
        VatNumber?   vatNumber        = null,
        string?      swissUid         = null,
        string?      sdiCode          = null,
        string?      iban             = null,
        string?      bicSwift         = null,
        int          paymentTermsDays = 30)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: name);

        return new Supplier
               {
                   Id           = Guid.NewGuid(),
                   TenantId     = tenantId,
                   Name         = name.Trim(),
                   Jurisdiction = jurisdiction,
                   Address      = address,
                   VatNumber    = vatNumber,
                   SwissUid     = swissUid,
                   SdiCode      = sdiCode,
                   Iban = iban?.Replace(
                                        oldValue: " ",
                                        newValue: string.Empty,
                                        comparisonType: StringComparison.Ordinal),
                   BicSwift         = bicSwift?.Trim().ToUpperInvariant(),
                   PaymentTermsDays = paymentTermsDays,
                   CreatedAtUtc     = DateTime.UtcNow,
               };
    }

}
