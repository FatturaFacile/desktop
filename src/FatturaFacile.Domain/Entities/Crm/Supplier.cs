// <copyright file="Supplier.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Crm;

/// <summary>
/// Represents supplier.
/// </summary>
public sealed class Supplier : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private Supplier()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the jurisdiction.
    /// </summary>
    public Jurisdiction Jurisdiction { get; private set; }

    /// <summary>
    /// Gets the vat number.
    /// </summary>
    public VatNumber? VatNumber { get; private set; }

    /// <summary>
    /// Gets the swiss uid.
    /// </summary>
    public string? SwissUid { get; private set; }

    /// <summary>
    /// Gets the sdi code.
    /// </summary>
    public string? SdiCode { get; private set; }

    /// <summary>
    /// Gets the address.
    /// </summary>
    public Address Address { get; private set; } = null!;

    /// <summary>
    /// Gets the iban.
    /// </summary>
    public string? Iban { get; private set; }

    /// <summary>
    /// Gets the bic swift.
    /// </summary>
    public string? BicSwift { get; private set; }

    /// <summary>
    /// Gets the payment terms days.
    /// </summary>
    public int PaymentTermsDays { get; private set; } = 30;

    /// <summary>
    /// Gets the notes.
    /// </summary>
    public string? Notes { get; }

    /// <summary>
    /// Gets a value indicating whether active.
    /// </summary>
    public bool IsActive { get; } = true;

    /// <summary>
    /// Gets or sets the created at utc.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the updated at utc.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the updated by.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="name">The name.</param>
    /// <param name="jurisdiction">The jurisdiction.</param>
    /// <param name="address">The address.</param>
    /// <param name="vatNumber">The vat number.</param>
    /// <param name="swissUid">The swiss uid.</param>
    /// <param name="sdiCode">The sdi code.</param>
    /// <param name="iban">The iban.</param>
    /// <param name="bicSwift">The bic swift.</param>
    /// <param name="paymentTermsDays">The payment terms days.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static Supplier Create(
        Guid tenantId,
        string name,
        Jurisdiction jurisdiction,
        Address address,
        VatNumber? vatNumber = null,
        string? swissUid = null,
        string? sdiCode = null,
        string? iban = null,
        string? bicSwift = null,
        int paymentTermsDays = 30)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Supplier
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name.Trim(),
            Jurisdiction = jurisdiction,
            Address = address,
            VatNumber = vatNumber,
            SwissUid = swissUid,
            SdiCode = sdiCode,
            Iban = iban?.Replace(
                " ",
                string.Empty,
                StringComparison.Ordinal),
            BicSwift = bicSwift?.Trim().ToUpperInvariant(),
            PaymentTermsDays = paymentTermsDays,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
