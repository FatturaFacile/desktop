using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Sales;

public sealed class SwissQrBill : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string BillNumber { get; private set; } = string.Empty;
    public DateTime BillDate { get; private set; }
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Creditor
    /// </summary>
    public string CreditorName { get; private set; } = string.Empty;
    public Address CreditorAddress { get; private set; } = null!;
    public QrIban CreditorQrIban { get; private set; } = null!;

    /// <summary>
    /// Debtor
    /// </summary>
    public string DebtorName { get; private set; } = string.Empty;
    public Address DebtorAddress { get; private set; } = null!;

    /// <summary>
    /// Amount & Currency (Must adhere to Swiss 0.05 CHF cash rounding rule when CHF)
    /// </summary>
    public Money Amount { get; private set; } = Money.Chf(0);
    public QrBillType BillType { get; private set; }

    /// <summary>
    /// 27-digit SCOR Reference (Structured Creditor Reference with Modulo 10 recursive checksum)
    /// </summary>
    public string ReferenceNumber { get; private set; } = string.Empty;
    public string? AdditionalUnstructuredMessage { get; private set; }
    public DateTime DueDate { get; private set; }
    public bool IsPaid { get; private set; }
    public DateTime? PaidDateUtc { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private SwissQrBill()
    {
    }

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

    public void MarkAsPaid(DateTime paidDate)
    {
        IsPaid = true;
        PaidDateUtc = paidDate;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}