namespace FatturaFacile.Invoicing.Domain.Enums;

public enum InvoiceType
{
    Standard = 1,
    SdiElectronic = 2,
    ChQrBill = 3,
    CreditNote = 4,
    DebitNote = 5,
    Proforma = 6
}

public enum PaymentStatus
{
    Unpaid = 1,
    PartiallyPaid = 2,
    Paid = 3,
    Overdue = 4,
    Cancelled = 5
}

public enum Jurisdiction
{
    IT = 1,
    CH = 2
}
