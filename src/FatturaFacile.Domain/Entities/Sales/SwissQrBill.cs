using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Sales;

/// <summary>
/// Represents swiss qr bill.
/// </summary>
public sealed class SwissQrBill : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the bill number.
    /// </summary>
    public string BillNumber { get; private set; } = string.Empty;

/// <summary>
    /// Gets the bill date.
    /// </summary>
    public DateTime BillDate { get; private set; }

/// <summary>
    /// Gets the client id.
    /// </summary>
    public Guid ClientId { get; private set; }

/// <summary>
    /// Gets the creditor.
    /// </summary>
    public string CreditorName { get; private set; } = string.Empty;

/// <summary>
    /// Gets the creditor address.
    /// </summary>
    public Address CreditorAddress { get; private set; } = null!;

/// <summary>
    /// Gets the creditor qr iban.
    /// </summary>
    public QrIban CreditorQrIban { get; private set; } = null!;

/// <summary>
    /// Gets the debtor.
    /// </summary>
    public string DebtorName { get; private set; } = string.Empty;

/// <summary>
    /// Gets the debtor address.
    /// </summary>
    public Address DebtorAddress { get; private set; } = null!;

/// <summary>
    /// Gets the amount &amp; Currency (Must adhere to Swiss 0.05 CHF cash rounding rule when CHF).
    /// </summary>
    public Money Amount { get; private set; } = Money.Chf(0);

/// <summary>
    /// Gets the bill type.
    /// </summary>
    public QrBillType BillType { get; private set; }

/// <summary>
    /// Gets the 27-digit SCOR Reference (Structured Creditor Reference with Modulo 10 recursive checksum).
    /// </summary>
    public string ReferenceNumber { get; private set; } = string.Empty;

/// <summary>
    /// Gets the additional unstructured message.
    /// </summary>
    public string? AdditionalUnstructuredMessage { get; private set; }

/// <summary>
    /// Gets the due date.
    /// </summary>
    public DateTime DueDate { get; private set; }

/// <summary>
    /// Gets a value indicating whether paid.
    /// </summary>
    public bool IsPaid { get; private set; }

/// <summary>
    /// Gets the paid date utc.
    /// </summary>
    public DateTime? PaidDateUtc { get; private set; }

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

private SwissQrBill()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="billNumber">The bill number.</param>
    /// <param name="billDate">The bill date.</param>
    /// <param name="clientId">The client id.</param>
    /// <param name="creditorName">The creditor name.</param>
    /// <param name="creditorAddress">The creditor address.</param>
    /// <param name="creditorQrIban">The creditor qr iban.</param>
    /// <param name="debtorName">The debtor name.</param>
    /// <param name="debtorAddress">The debtor address.</param>
    /// <param name="rawAmount">The raw amount.</param>
    /// <param name="referenceNumber">The reference number.</param>
    /// <param name="dueDate">The due date.</param>
    /// <param name="message">The message.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static SwissQrBill Create(
        Guid tenantId,
        string billNumber,
        DateTime billDate,
        Guid clientId,
        string creditorName,
        Address creditorAddress,
        QrIban creditorQrIban,
        string debtorName,
        Address debtorAddress,
        Money rawAmount,
        string referenceNumber,
        DateTime dueDate,
        string? message = null)
    {
        // Apply statutory 0.05 CHF rounding
        var finalAmount = rawAmount.ToSwissCashRounding();

        return new SwissQrBill
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BillNumber = billNumber,
            BillDate = billDate,
            ClientId = clientId,
            CreditorName = creditorName,
            CreditorAddress = creditorAddress,
            CreditorQrIban = creditorQrIban,
            DebtorName = debtorName,
            DebtorAddress = debtorAddress,
            Amount = finalAmount,
            BillType = QrBillType.QrIbanWithReference,
            ReferenceNumber = referenceNumber.Replace(" ", string.Empty, StringComparison.Ordinal),
            AdditionalUnstructuredMessage = message,
            DueDate = dueDate,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

/// <summary>
    /// Mark As Paid.
    /// </summary>
    /// <param name="paidDate">The paid date.</param>
    public void MarkAsPaid(DateTime paidDate)
    {
        IsPaid = true;
        PaidDateUtc = paidDate;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
