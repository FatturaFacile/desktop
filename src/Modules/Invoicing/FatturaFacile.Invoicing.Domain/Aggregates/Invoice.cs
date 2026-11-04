using FatturaFacile.Domain.Shared.Primitives;
using FatturaFacile.Invoicing.Domain.Entities;
using FatturaFacile.Invoicing.Domain.Enums;
using FatturaFacile.Invoicing.Domain.Events;

namespace FatturaFacile.Invoicing.Domain.Aggregates;

public sealed class Invoice : AggregateRoot<Guid>, IHasTenant, ISoftDeletable, IAuditableEntity
{
    private readonly List<InvoiceItem> _items = [];

    public Guid TenantId { get; private set; }
    public Guid ContactId { get; private set; }
    public InvoiceType InvoiceType { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public DateOnly Date { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public string Currency { get; private set; } = "EUR";
    public decimal ExchangeRate { get; private set; } = 1.0m;

    public decimal TotalNet { get; private set; }
    public decimal TotalTax { get; private set; }
    public decimal TotalGross { get; private set; }
    public decimal RoundingAmount { get; private set; }
    public decimal TotalPaid { get; private set; }

    public PaymentStatus PaymentStatus { get; private set; }
    public Jurisdiction Jurisdiction { get; private set; }
    public bool SplitPayment { get; private set; }
    public bool ReverseCharge { get; private set; }
    public string? Notes { get; private set; }

    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    // Audit
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    private Invoice() { }

    public static Invoice Create(
        Guid tenantId,
        Guid contactId,
        InvoiceType invoiceType,
        string invoiceNumber,
        DateOnly date,
        DateOnly? dueDate,
        string currency,
        Jurisdiction jurisdiction,
        bool splitPayment = false,
        bool reverseCharge = false,
        string? notes = null,
        decimal exchangeRate = 1.0m)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID is required.", nameof(tenantId));
        if (contactId == Guid.Empty) throw new ArgumentException("Contact ID is required.", nameof(contactId));
        if (string.IsNullOrWhiteSpace(invoiceNumber)) throw new ArgumentException("Invoice number is required.", nameof(invoiceNumber));
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3) throw new ArgumentException("Currency must be 3 letters.", nameof(currency));

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ContactId = contactId,
            InvoiceType = invoiceType,
            InvoiceNumber = invoiceNumber.Trim(),
            Date = date,
            DueDate = dueDate,
            Currency = currency.ToUpperInvariant(),
            Jurisdiction = jurisdiction,
            SplitPayment = splitPayment,
            ReverseCharge = reverseCharge,
            Notes = notes?.Trim(),
            ExchangeRate = exchangeRate > 0 ? exchangeRate : 1.0m,
            PaymentStatus = PaymentStatus.Unpaid,
            TotalPaid = 0m,
            CreatedAt = DateTimeOffset.UtcNow
        };

        return invoice;
    }

    public void AddItem(
        Guid? productId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal taxRate,
        decimal discountPercentage = 0m)
    {
        var item = InvoiceItem.Create(
            TenantId,
            productId,
            description,
            quantity,
            unitPrice,
            taxRate,
            discountPercentage);

        _items.Add(item);
        RecalculateTotals();
    }

    public void RecalculateTotals()
    {
        TotalNet = decimal.Round(_items.Sum(i => i.LineTotal), 2, MidpointRounding.AwayFromZero);
        TotalTax = decimal.Round(_items.Sum(i => i.TaxAmount), 2, MidpointRounding.AwayFromZero);

        var unroundedGross = TotalNet + TotalTax;
        
        // Swiss CHF 0.05 rounding rule (round to nearest 0.05 CHF)
        if (string.Equals(Currency, "CHF", StringComparison.OrdinalIgnoreCase))
        {
            TotalGross = decimal.Round(unroundedGross * 20m, 0, MidpointRounding.AwayFromZero) / 20m;
            RoundingAmount = decimal.Round(TotalGross - unroundedGross, 2, MidpointRounding.AwayFromZero);
        }
        else
        {
            TotalGross = unroundedGross;
            RoundingAmount = 0m;
        }

        RaiseDomainEvent(new InvoiceCreatedDomainEvent(Id, TenantId, InvoiceNumber, TotalGross, Currency));
    }

    public void RegisterPayment(decimal amount, DateTimeOffset paidAt)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

        TotalPaid = decimal.Round(TotalPaid + amount, 2, MidpointRounding.AwayFromZero);

        var oldStatus = PaymentStatus.ToString();
        if (TotalPaid >= TotalGross)
        {
            PaymentStatus = PaymentStatus.Paid;
        }
        else
        {
            PaymentStatus = PaymentStatus.PartiallyPaid;
        }

        RaiseDomainEvent(new InvoicePaidDomainEvent(Id, TenantId, amount, paidAt));
        RaiseDomainEvent(new InvoiceStatusChangedDomainEvent(Id, oldStatus, PaymentStatus.ToString()));
    }

    public void Cancel(string reason)
    {
        if (PaymentStatus == PaymentStatus.Paid)
            throw new InvalidOperationException("Cannot cancel an invoice that is already fully paid. Issue a Credit Note instead.");

        var oldStatus = PaymentStatus.ToString();
        PaymentStatus = PaymentStatus.Cancelled;
        Notes = string.IsNullOrWhiteSpace(Notes) ? $"Cancelled: {reason}" : $"{Notes} | Cancelled: {reason}";
        
        RaiseDomainEvent(new InvoiceStatusChangedDomainEvent(Id, oldStatus, PaymentStatus.Cancelled.ToString()));
    }
}
