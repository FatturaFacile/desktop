namespace FatturaFacile.Domain.Enums;

public enum Currency
{
    EUR = 1,
    CHF = 2,
    USD = 3,
    GBP = 4,
}

public enum Jurisdiction
{
    Italy = 1,
    Switzerland = 2,
    CrossBorder = 3,
}

public enum InvoiceType
{
    Sales = 1,
    Purchase = 2,
    CreditNote = 3,
    DebitNote = 4,
    FeeNote = 5,
}

public enum SdiTransmissionStatus
{
    Draft = 0,
    ReadyToSend = 1,
    Sent = 2,
    Delivered = 3,
    Failed = 4,
    Rejected = 5,
}

public enum QrBillType
{
    QrIbanWithReference = 1,
    IbanWithScor = 2,
    IbanWithoutReference = 3,
}

public enum TireSeason
{
    Summer = 1,
    Winter = 2,
    AllSeason = 3,
}

public enum WorkerType
{
    Employee = 1,
    CrossBorderFrontaliere = 2,
    Subcontractor = 3,
    Adviser = 4,
}

public enum UserRole
{
    SuperAdmin = 1,
    Admin = 2,
    Accountant = 3,
    FieldTechnician = 4,
    SalesRepresentative = 5,
    HrManager = 6,
}

public enum MovementType
{
    Receipt = 1,
    Issue = 2,
    Transfer = 3,
    Adjustment = 4,
}

public enum InspectionType
{
    ItalianRevisione = 1,
    SwissMfkCollaudo = 2,
}

public enum DamageSeverity
{
    Minor = 1,
    Moderate = 2,
    Severe = 3,
}

public enum StandardType
{
    ItalianComputo = 1,
    SwissSiaCrbNpk = 2,
}

public enum RiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
}

public enum ProductionStatus
{
    Planned = 1,
    InProduction = 2,
    Completed = 3,
    Cancelled = 4,
}

public enum PaymentMethod
{
    Cash = 1,
    CardSumUpWorldline = 2,
    BankTransfer = 3,
    DigitalVoucher = 4,
}

public enum RecordingType
{
    Detailed = 1,
    SimplifiedSwissVereinfacht = 2,
}

public enum AbsenceType
{
    Ferie = 1,
    Rol = 2,
    Legge104 = 3,
    InpsIllnessPuc = 4,
    SwissKtgMedical = 5,
    SwissApgMilitaryService = 6,
}

public enum VatSettlementPeriodType
{
    Monthly = 1,
    Quarterly = 2,
}

public enum AccountCategory
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5,
}
