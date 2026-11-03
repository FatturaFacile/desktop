// <copyright file="TechnicianWorkOrder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.ServiceTickets;

/// <summary>
/// Represents technician work order.
/// </summary>
public sealed class TechnicianWorkOrder : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{

    /// <summary>
    /// Initializes a new instance of the <see cref="TechnicianWorkOrder"/> class for entity framework or serialization.
    /// </summary>
    private TechnicianWorkOrder()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the rAP-2026/001.
    /// </summary>
    public string WorkOrderNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the support ticket id.
    /// </summary>
    public Guid SupportTicketId { get; private set; }

    /// <summary>
    /// Gets the technician user id.
    /// </summary>
    public Guid TechnicianUserId { get; private set; }

    /// <summary>
    /// Gets the service date.
    /// </summary>
    public DateTime ServiceDate { get; private set; }

    /// <summary>
    /// Gets the hours worked.
    /// </summary>
    public decimal HoursWorked { get; private set; }

    /// <summary>
    /// Gets the hourly rate.
    /// </summary>
    public decimal HourlyRate { get; private set; }

    /// <summary>
    /// Gets the currency.
    /// </summary>
    public Currency Currency { get; private set; } = Currency.EUR;

    /// <summary>
    /// Gets the work performed summary.
    /// </summary>
    public string WorkPerformedSummary { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the spare parts used json.
    /// </summary>
    public string? SparePartsUsedJson { get; private set; }

    /// <summary>
    /// Gets the customer Signoff &amp; Signature.
    /// </summary>
    public string? CustomerSignatoryName { get; private set; }

    /// <summary>
    /// Gets the digital signature svg or base64.
    /// </summary>
    public string? DigitalSignatureSvgOrBase64 { get; private set; }

    /// <summary>
    /// Gets a value indicating whether signed by customer.
    /// </summary>
    public bool IsSignedByCustomer { get; private set; }

    /// <summary>
    /// Gets the signed at utc.
    /// </summary>
    public DateTime? SignedAtUtc { get; private set; }

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
    /// Creates a new technician work order instance.
    /// </summary>
    /// <param name="data">The parameter object containing technician work order details.</param>
    /// <returns>A newly created <see cref="TechnicianWorkOrder"/> instance.</returns>
    public static TechnicianWorkOrder Create(
        TechnicianWorkOrderData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return new TechnicianWorkOrder
        {
            Id                   = Guid.NewGuid(),
            TenantId             = data.TenantId,
            WorkOrderNumber      = data.WorkOrderNumber,
            SupportTicketId      = data.TicketId,
            TechnicianUserId     = data.TechnicianId,
            ServiceDate          = data.ServiceDate,
            HoursWorked          = data.HoursWorked,
            HourlyRate           = data.HourlyRate,
            Currency             = data.Currency,
            WorkPerformedSummary = data.Summary,
            SparePartsUsedJson   = data.SpareParts,
            CreatedAtUtc         = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Sign By Customer.
    /// </summary>
    /// <param name="signatoryName">The signatory name.</param>
    /// <param name="signatureData">The signature data.</param>
    public void SignByCustomer(
        string signatoryName,
        string signatureData)
    {
        CustomerSignatoryName       = signatoryName;
        DigitalSignatureSvgOrBase64 = signatureData;
        IsSignedByCustomer          = true;
        SignedAtUtc                 = DateTime.UtcNow;
        UpdatedAtUtc                = DateTime.UtcNow;
    }

}
