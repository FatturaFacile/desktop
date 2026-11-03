// <copyright file="RetailReceipt.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.RetailPos;

/// <summary>
/// Represents a retail receipt.
/// </summary>
public sealed class RetailReceipt : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetailReceipt"/> class.
    /// </summary>
    private RetailReceipt()
    {
    }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the POS terminal session identifier.
    /// </summary>
    public Guid PosTerminalSessionId { get; private set; }

    /// <summary>
    /// Gets the receipt number.
    /// </summary>
    public string ReceiptNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the receipt timestamp in UTC.
    /// </summary>
    public DateTime TimestampUtc { get; private set; }

    /// <summary>
    /// Gets the total receipt amount.
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Gets the receipt currency.
    /// </summary>
    public Currency Currency { get; private set; } = Currency.EUR;

    /// <summary>
    /// Gets the payment method.
    /// </summary>
    public PaymentMethod PaymentMethod { get; private set; }

    /// <summary>
    /// Gets the Italian RT transmission lot.
    /// </summary>
    public string? ItalianRtTransmissionLot { get; private set; }

    /// <summary>
    /// Gets the Italian Z report number.
    /// </summary>
    public string? ItalianZReportNumber { get; private set; }

    /// <summary>
    /// Gets the Swiss payment terminal transaction reference.
    /// </summary>
    public string? SwissTerminalTxReference { get; private set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the creator.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the last editor.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a retail receipt.
    /// </summary>
    /// <param name="tenantId">Tenant identifier.</param>
    /// <param name="data">Receipt creation data.</param>
    /// <returns>A new retail receipt.</returns>
    public static RetailReceipt Create(
        Guid tenantId,
        RetailReceiptCreateData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return new RetailReceipt
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PosTerminalSessionId = data.PosTerminalSessionId,
            ReceiptNumber = data.ReceiptNumber.Trim(),
            TimestampUtc = DateTime.UtcNow,
            TotalAmount = data.TotalAmount,
            Currency = data.Currency,
            PaymentMethod = data.PaymentMethod,
            ItalianRtTransmissionLot = data.ItalianRtTransmissionLot,
            ItalianZReportNumber = data.ItalianZReportNumber,
            SwissTerminalTxReference = data.SwissTerminalTxReference,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
