// <copyright file="RetailReceipt.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.RetailPos;

/// <summary>
/// Represents retail receipt.
/// </summary>
public sealed class RetailReceipt : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private RetailReceipt()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the pos terminal session id.
    /// </summary>
    public Guid PosTerminalSessionId { get; private set; }

    /// <summary>
    /// Gets the receipt number.
    /// </summary>
    public string ReceiptNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the timestamp utc.
    /// </summary>
    public DateTime TimestampUtc { get; private set; }

    /// <summary>
    /// Gets the total amount.
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Gets the currency.
    /// </summary>
    public Currency Currency { get; private set; } = Currency.EUR;

    /// <summary>
    /// Gets the payment method.
    /// </summary>
    public PaymentMethod PaymentMethod { get; private set; }

    /// <summary>
    /// Gets the electronic Cash Register (RT - Registratore Telematico) / Fiscal lot mapping.
    /// </summary>
    public string? ItalianRtTransmissionLot { get; private set; }

    /// <summary>
    /// Gets the italian z report number.
    /// </summary>
    public string? ItalianZReportNumber { get; private set; }

    /// <summary>
    /// Gets the swiss terminal tx reference.
    /// </summary>
    public string? SwissTerminalTxReference { get; private set; }

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
    /// <param name="sessionId">The session id.</param>
    /// <param name="receiptNumber">The receipt number.</param>
    /// <param name="amount">The amount.</param>
    /// <param name="currency">The currency.</param>
    /// <param name="paymentMethod">The payment method.</param>
    /// <param name="rtLot">The rt lot.</param>
    /// <param name="zReport">The z report.</param>
    /// <param name="chTerminalTx">The ch terminal tx.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static RetailReceipt Create(
        Guid tenantId,
        Guid sessionId,
        string receiptNumber,
        decimal amount,
        Currency currency,
        PaymentMethod paymentMethod,
        string? rtLot = null,
        string? zReport = null,
        string? chTerminalTx = null)
    {
        return new RetailReceipt
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PosTerminalSessionId = sessionId,
            ReceiptNumber = receiptNumber?.Trim() ?? string.Empty,
            TimestampUtc = DateTime.UtcNow,
            TotalAmount = amount,
            Currency = currency,
            PaymentMethod = paymentMethod,
            ItalianRtTransmissionLot = rtLot,
            ItalianZReportNumber = zReport,
            SwissTerminalTxReference = chTerminalTx,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
