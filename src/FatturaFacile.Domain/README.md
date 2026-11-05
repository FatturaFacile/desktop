# FatturaFacile.Domain

Core enterprise domain layer for the FatturaFacile desktop ERP solution.

## 🎯 Architectural Purpose & Rules
- **Layer Index:** Layer 1 (innermost clean layer).
- **External Dependencies:** **ZERO**. This project does not reference Entity Framework, MediatR, ASP.NET Core, or WPF.
- **Tactical DDD Mandate:** **STRICTLY PROHIBITED**. Entities are clean, pragmatic POCOs with public parameterless constructors and direct property accessors (`{ get; set; }`). No aggregate roots, no domain event raising, no reflection-based value objects.

---

## 📁 Project Structure

```text
FatturaFacile.Domain/
├── Common/
│   ├── IBaseEntity.cs              # Base auditing contract (Id, CreatedAt, UpdatedAt, DeletedAt)
│   ├── ITenantScopedEntity.cs      # Multi-tenant contract with mandatory TenantId
│   ├── IOptionalTenantEntity.cs    # Multi-tenant contract with nullable TenantId (shared defaults)
│   ├── ICurrentTenantService.cs    # Service contract resolving active TenantId
│   └── Money.cs                    # Lightweight record (Amount, Currency)
├── Entities/
│   ├── Tenant.cs                   # Enterprise tenant company details and fiscal IDs
│   ├── Contact.cs                  # Customers, suppliers, and fiduciaries
│   ├── Product.cs                  # Catalog products and service definitions
│   ├── Invoice.cs                  # Core commercial sales invoice document
│   ├── InvoiceItem.cs              # Invoice line item details
│   ├── InvoiceQrBillData.cs        # Swiss ISO 20022 QR-Bill compliance metadata
│   ├── InvoiceSdiMetadata.cs       # Italian SDI / FatturaPA electronic invoicing metadata
│   ├── VatCode.cs                  # Standard and localized VAT rates and exemptions
│   ├── ChartOfAccount.cs           # General ledger accounting plan (COA)
│   ├── GeneralLedgerEntry.cs       # Double-entry ledger journal header (Prima Nota)
│   ├── GeneralLedgerLine.cs        # Journal line (Debit / Credit)
│   ├── HrEmployee.cs               # Employee payroll profile and social security data
│   ├── Warehouse.cs                # Inventory storage facility
│   └── StockItem.cs                # Real-time physical inventory tracking
└── Enums/
    ├── CountryCode.cs              # CH (Switzerland), IT (Italy)
    ├── InvoiceType.cs              # standard, proforma, credit_note, debit_note
    ├── InvoiceStatus.cs            # Draft, Issued, Paid, Cancelled
    ├── PaymentStatus.cs            # Pending, PartiallyPaid, Paid, Overdue
    ├── AccountType.cs              # Asset, Liability, Equity, Revenue, Expense
    ├── NormalBalance.cs            # Debit, Credit
    ├── SdiStatus.cs                # NotSent, Transmitted, Delivered, Rejected
    ├── ContactType.cs              # Customer, Supplier, Both
    ├── EmploymentType.cs           # FullTime, PartTime, Contractor
    └── EmploymentStatus.cs         # Active, OnLeave, Terminated
```

---

## 🔑 Multi-Tenancy Hierarchy

### `ITenantScopedEntity`
- Mandates a non-nullable `Guid TenantId { get; set; }`.
- Applied to all tenant-private transactional and accounting records: `Invoice`, `InvoiceItem`, `GeneralLedgerEntry`, `HrEmployee`, `StockItem`.
- Strictly isolated by EF Core global query filters.

### `IOptionalTenantEntity`
- Features a nullable `Guid? TenantId { get; set; }`.
- Applied to reference tables and customizable templates: `VatCode`, `ChartOfAccount`, `Contact`, `Product`.
- **Shared Defaults:** When `TenantId == null`, the entity is a global system default shared across all tenants and protected against modification by `TenantSaveChangesInterceptor`.

---

## 💰 The `Money` Record

Monetary values are represented by the positional record:

```csharp
public sealed record Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency) => new(0m, currency);
}
```
- Avoids complex reflection-based ValueObject overhead.
- Supports effortless equality comparison, pattern matching, and JSON serialization.
