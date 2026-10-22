// <copyright file="PassiveInvoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Purchasing;

/// <summary>
/// Represents an inbound passive vendor invoice (fattura passiva / Lieferantenrechnung) received from a supplier,
/// supporting Italian SDI reception and Swiss QR-bill payloads, financial tracking, and bank reconciliation.
/// </summary>
public sealed class PassiveInvoice : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private PassiveInvoice()
    {
    }

    /// <summary>
    /// Gets or sets the unique tenant identifier for multi-tenant data partitioning.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique identifier of the vendor or supplier in the purchasing ledger.
    /// </summary>
    public Guid SupplierId { get; private set; }

    /// <summary>
    /// Gets the supplier's original billing document reference number as printed or transmitted on the invoice.
    /// </summary>
    public string DocumentNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the date on which the supplier issued the invoice.
    /// </summary>
    public DateTime DocumentDate { get; private set; }

    /// <summary>
    /// Gets the total net taxable purchase amount before value-added tax.
    /// </summary>
    public decimal TotalNet { get; private set; }

    /// <summary>
    /// Gets the total deductible or non-deductible input VAT billed by the supplier.
    /// </summary>
    public decimal TotalVat { get; private set; }

    /// <summary>
    /// Gets the overall gross invoice amount payable to the supplier (<see cref="TotalNet"/> plus <see cref="TotalVat"/>).
    /// </summary>
    public decimal TotalGross { get; private set; }

    /// <summary>
    /// Gets the ISO billing currency of the vendor invoice.
    /// </summary>
    public Currency Currency { get; private set; }

    /// <summary>
    /// Gets the contractual maturity or due date for payment settlement.
    /// </summary>
    public DateTime PaymentDueDate { get; private set; }

    /// <summary>
    /// Gets the official Italian SDI interchange identifier (IdentificativoSdI) if received electronically through the Agenzia delle Entrate pipeline.
    /// </summary>
    public string? SdiIdentifier { get; private set; }

    /// <summary>
    /// Gets the full Swiss QR-bill QR-code text payload or Swiss payment slip data if ingested as a QR invoice.
    /// </summary>
    public string? SwissQrPayload { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this vendor invoice has been fully matched and reconciled against a bank statement transaction line.
    /// </summary>
    public bool IsReconciled { get; private set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the record was created in the system.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user or process that created the record.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the record was last modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user or process that last modified the record.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a new <see cref="PassiveInvoice"/> aggregate representing a received vendor invoice.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="supplierId">The supplier or vendor identifier.</param>
    /// <param name="documentNumber">The vendor's invoice number.</param>
    /// <param name="documentDate">The statutory invoice date.</param>
    /// <param name="totalNet">The total net purchase amount excluding tax.</param>
    /// <param name="totalVat">The total input VAT amount.</param>
    /// <param name="currency">The currency of the invoice.</param>
    /// <param name="paymentDueDate">The payment due date.</param>
    /// <param name="sdiId">The Italian SDI transmission identifier, if applicable.</param>
    /// <param name="qrPayload">The Swiss QR-code raw text payload, if applicable.</param>
    /// <returns>A newly instantiated <see cref="PassiveInvoice"/> aggregate.</returns>
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
    /// Marks the vendor invoice as reconciled against matching bank payment debits or treasury movements.
    /// </summary>
    public void MarkReconciled()
    {
        IsReconciled = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
