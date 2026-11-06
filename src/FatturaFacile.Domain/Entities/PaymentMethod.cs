// <copyright file="PaymentMethod.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents a supported financial payment channel or instrument (e.g. Bonifico Bancario, QR-Bill, Ri.Ba, Cash).
/// Supports global system defaults (TenantId == null) and custom tenant methods.
/// </summary>
public class PaymentMethod : IOptionalTenantEntity
{

    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the optional owning tenant identifier. If null, this payment method is global.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the unique code identifier (e.g., "BANK_TRANSFER", "SWISS_QR", "RIBA", "CASH").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the payment instrument is electronic.
    /// </summary>
    public bool IsElectronic { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether an IBAN is required for this method.
    /// </summary>
    public bool RequiresIban { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this method is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
