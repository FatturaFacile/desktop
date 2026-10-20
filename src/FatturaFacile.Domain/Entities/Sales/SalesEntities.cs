using System.Globalization;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Sales;

public sealed class ItalianElectronicInvoice : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string DocumentNumber { get; private set; } = string.Empty;
    public DateTime DocumentDate { get; private set; }
    /// <summary>
    /// FPR12 = B2B / Privati; FPA12 = Public Administration
    /// </summary>
    public string FormatoTrasmissione { get; } = "FPR12";
    /// <summary>
    /// Unique SDI submission counter
    /// </summary>
    public string ProgressivoInvio { get; private set; } = string.Empty;
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Header Data
    /// </summary>
    public string CedenteDenominazione { get; private set; } = string.Empty;
    public string CedentePartitaIva { get; private set; } = string.Empty;
    public string CedenteCodiceFiscale { get; private set; } = string.Empty;
    public Address CedenteIndirizzo { get; private set; } = null!;

    public string CessionarioDenominazione { get; private set; } = string.Empty;
    public string? CessionarioPartitaIva { get; private set; }
    public string? CessionarioCodiceFiscale { get; private set; }
    public string CessionarioCodiceDestinatario { get; private set; } = "0000000";
    public string? CessionarioPec { get; private set; }
    public Address CessionarioIndirizzo { get; private set; } = null!;

    /// <summary>
    /// Financial Totals
    /// </summary>
    public Currency Currency { get; } = Currency.EUR;
    public decimal TotalNet { get; private set; }
    public decimal TotalVat { get; private set; }
    public decimal TotalGross { get; private set; }
    /// <summary>
    /// MP05 = Bonifico
    /// </summary>
    public string PaymentMethodCode { get; } = "MP05";
    public DateTime PaymentDueDate { get; private set; }

    /// <summary>
    /// SDI Status
    /// </summary>
    public SdiTransmissionStatus SdiStatus { get; private set; } = SdiTransmissionStatus.Draft;
    public string? SdiIdentifier { get; private set; }
    public string? SdiErrorMessage { get; private set; }
    public DateTime? SdiSentAtUtc { get; private set; }
    public DateTime? SdiDeliveredAtUtc { get; private set; }

    private readonly List<InvoiceLine> _lines = [];
    public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private ItalianElectronicInvoice()
    {
    }

    public static ItalianElectronicInvoice Create(
        Guid tenantId,
        string documentNumber,
        DateTime documentDate,
        Guid clientId,
        string cedenteName,
        string cedenteVat,
        string cedenteCf,
        Address cedenteAddress,
        string cessionarioName,
        string? cessionarioVat,
        string? cessionarioCf,
        string sdiCode,
        string? pec,
        Address cessionarioAddress,
        DateTime dueDate)
    {
        return new ItalianElectronicInvoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DocumentNumber = documentNumber,
            DocumentDate = documentDate,
            ProgressivoInvio = string.Create(CultureInfo.InvariantCulture, $"{DateTime.UtcNow:yyyyMMddHHmmssfff}"),
            ClientId = clientId,
            CedenteDenominazione = cedenteName,
            CedentePartitaIva = cedenteVat,
            CedenteCodiceFiscale = cedenteCf,
            CedenteIndirizzo = cedenteAddress,
            CessionarioDenominazione = cessionarioName,
            CessionarioPartitaIva = cessionarioVat,
            CessionarioCodiceFiscale = cessionarioCf,
            CessionarioCodiceDestinatario = string.IsNullOrWhiteSpace(sdiCode) ? "0000000" : sdiCode.Trim().ToUpperInvariant(),
            CessionarioPec = pec,
            CessionarioIndirizzo = cessionarioAddress,
            PaymentDueDate = dueDate,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void AddLine(string description, decimal quantity, decimal unitPrice, decimal vatRate, string? natura = null)
    {
        var line = new InvoiceLine(_lines.Count + 1, description, quantity, unitPrice, vatRate, natura);
        _lines.Add(line);
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        TotalNet = _lines.Sum(l => l.LineTotal);
        TotalVat = _lines.Sum(l => decimal.Round(l.LineTotal * (l.VatRate / 100m), 2, MidpointRounding.AwayFromZero));
        TotalGross = TotalNet + TotalVat;
    }

    public void MarkSentToSdi(string sdiIdentifier)
    {
        SdiStatus = SdiTransmissionStatus.Sent;
        SdiIdentifier = sdiIdentifier;
        SdiSentAtUtc = DateTime.UtcNow;
    }

    public void MarkDelivered()
    {
        SdiStatus = SdiTransmissionStatus.Delivered;
        SdiDeliveredAtUtc = DateTime.UtcNow;
    }

    public void MarkRejected(string errorMessage)
    {
        SdiStatus = SdiTransmissionStatus.Rejected;
        SdiErrorMessage = errorMessage;
    }
}

public sealed class InvoiceLine : Entity<int>
{
    public int LineNumber { get; }
    public string Description { get; } = string.Empty;
    public decimal Quantity { get; }
    public decimal UnitPrice { get; }
    /// <summary>
    /// e.g. 22.0
    /// </summary>
    public decimal VatRate { get; }
    /// <summary>
    /// e.g. N1, N2.1, N3.1, N4
    /// </summary>
    public string? NaturaExemptionCode { get; }
    public decimal LineTotal => decimal.Round(Quantity * UnitPrice, 2, MidpointRounding.AwayFromZero);

    internal InvoiceLine(int lineNumber, string description, decimal quantity, decimal unitPrice, decimal vatRate, string? natura)
    {
        Id = lineNumber;
        LineNumber = lineNumber;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        VatRate = vatRate;
        NaturaExemptionCode = natura;
    }

    private InvoiceLine()
    {
    }
}

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

public sealed class SalesOrder : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid ClientId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public Currency Currency { get; private set; } = Currency.EUR;
    public decimal TotalAmount { get; }
    public string Status { get; } = "Confirmed";

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private SalesOrder()
    {
    }

    public static SalesOrder Create(Guid tenantId, string orderNumber, Guid clientId, Currency currency)
    {
        return new SalesOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderNumber = orderNumber,
            ClientId = clientId,
            OrderDate = DateTime.UtcNow,
            Currency = currency,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}

public sealed class Quote : AggregateRoot<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string QuoteNumber { get; private set; } = string.Empty;
    public Guid ClientId { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public Money TotalAmount { get; private set; } = Money.Euro(0);
    public bool IsAccepted { get; private set; }

    private Quote()
    {
    }

    public static Quote Create(Guid tenantId, string quoteNumber, Guid clientId, DateTime expiry, Money total)
    {
        return new Quote
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            QuoteNumber = quoteNumber,
            ClientId = clientId,
            ExpiryDate = expiry,
            TotalAmount = total,
            IsAccepted = false,
        };
    }
}

public sealed class PriceList : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public Currency Currency { get; private set; }
    public bool IsDefault { get; private set; }

    private PriceList()
    {
    }

    public static PriceList Create(Guid tenantId, string name, Currency currency, bool isDefault = false)
    {
        return new PriceList
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Currency = currency,
            IsDefault = isDefault,
        };
    }
}
