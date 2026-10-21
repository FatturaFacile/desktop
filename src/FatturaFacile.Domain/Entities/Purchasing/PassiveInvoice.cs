// <copyright file="PassiveInvoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Purchasing;

/// <summary>
/// Represents passive invoice.
/// </summary>
public sealed class PassiveInvoice : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private PassiveInvoice()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the supplier id.
    /// </summary>
    public Guid SupplierId { get; private set; }

    /// <summary>
    /// Gets the document number.
    /// </summary>
    public string DocumentNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the document date.
    /// </summary>
    public DateTime DocumentDate { get; private set; }

    /// <summary>
    /// Gets the total net.
    /// </summary>
    public decimal TotalNet { get; private set; }

    /// <summary>
    /// Gets the total vat.
    /// </summary>
    public decimal TotalVat { get; private set; }

    /// <summary>
    /// Gets the total gross.
    /// </summary>
    public decimal TotalGross { get; private set; }

    /// <summary>
    /// Gets the currency.
    /// </summary>
    public Currency Currency { get; private set; }

    /// <summary>
    /// Gets the payment due date.
    /// </summary>
    public DateTime PaymentDueDate { get; private set; }

    /// <summary>
    /// Gets the sdi identifier.
    /// </summary>
    public string? SdiIdentifier { get; private set; }

    /// <summary>
    /// Gets the swiss qr payload.
    /// </summary>
    public string? SwissQrPayload { get; private set; }

    /// <summary>
    /// Gets a value indicating whether reconciled.
    /// </summary>
    public bool IsReconciled { get; private set; }

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
    /// <param name="supplierId">The supplier id.</param>
    /// <param name="documentNumber">The document number.</param>
    /// <param name="documentDate">The document date.</param>
    /// <param name="totalNet">The total net.</param>
    /// <param name="totalVat">The total vat.</param>
    /// <param name="currency">The currency.</param>
    /// <param name="paymentDueDate">The payment due date.</param>
    /// <param name="sdiId">The sdi id.</param>
    /// <param name="qrPayload">The qr payload.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static PassiveInvoice Create(
        Guid tenantId,
        Guid supplierId,
        string documentNumber,
        DateTime documentDate,
        decimal totalNet,
        decimal totalVat,
        Currency currency,
        DateTime paymentDueDate,
        string? sdiId = null,
        string? qrPayload = null)
    {
        return new PassiveInvoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierId = supplierId,
            DocumentNumber = documentNumber,
            DocumentDate = documentDate,
            TotalNet = totalNet,
            TotalVat = totalVat,
            TotalGross = totalNet + totalVat,
            Currency = currency,
            PaymentDueDate = paymentDueDate,
            SdiIdentifier = sdiId,
            SwissQrPayload = qrPayload,
            IsReconciled = false,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Mark Reconciled.
    /// </summary>
    public void MarkReconciled()
    {
        IsReconciled = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
