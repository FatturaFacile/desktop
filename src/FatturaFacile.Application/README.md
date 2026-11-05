# FatturaFacile.Application

Application orchestration and CQRS business logic layer for the FatturaFacile desktop ERP solution.

## 🎯 Architectural Purpose & Rules
- **Layer Index:** Layer 2 (clean orchestration layer).
- **Dependencies:** References `FatturaFacile.Domain`.
- **Primary Technologies:** MediatR, FluentValidation, AutoMapper.
- **Rules:** No direct database connections (uses repository abstractions) and no UI framework references.

---

## 📁 Project Structure

```text
FatturaFacile.Application/
├── ApplicationServiceRegistration.cs   # DI extension method AddApplicationServices()
├── Common/
│   ├── Behaviors/
│   │   └── ValidationBehavior.cs       # MediatR IPipelineBehavior executing FluentValidation
│   ├── Exceptions/
│   │   └── ValidationException.cs      # Custom exception carrying validation failure dictionaries
│   ├── Interfaces/
│   │   ├── IRepository.cs              # Generic repository contract
│   │   ├── IUnitOfWork.cs              # Transaction management contract
│   │   ├── IDocumentExportService.cs   # PDF / Excel generation contract
│   │   └── IUserAuthenticationService.cs # Biometric / Windows Hello authentication contract
│   └── Mappings/
│       └── MappingProfile.cs           # AutoMapper profile configuring DTO mappings
├── DTOs/                               # Data Transfer Objects (Projections for ViewModels)
│   ├── InvoiceDto.cs
│   ├── ContactDto.cs
│   ├── ProductDto.cs
│   ├── ChartOfAccountDto.cs
│   ├── GeneralLedgerEntryDto.cs
│   └── PayrollCalculationResultDto.cs
├── Features/                           # CQRS Vertical Slices
│   ├── Invoicing/
│   │   ├── Commands/                   # CreateInvoiceCommand, IssueInvoiceCommand, DeleteInvoiceCommand
│   │   └── Queries/                    # GetInvoicesQuery, GetInvoiceByIdQuery
│   ├── Accounting/
│   │   ├── Commands/                   # CreateGeneralLedgerEntryCommand
│   │   └── Queries/                    # GetChartOfAccountsQuery, GetGeneralLedgerEntriesQuery
│   ├── Contacts/
│   │   ├── Commands/                   # CreateContactCommand, UpdateContactCommand, DeleteContactCommand
│   │   └── Queries/                    # GetContactsQuery
│   ├── Products/
│   │   ├── Commands/                   # CreateProductCommand
│   │   └── Queries/                    # GetProductsQuery
│   ├── Payroll/
│   │   ├── Commands/                   # CreateEmployeeCommand
│   │   └── Queries/                    # CalculatePayrollPreviewQuery, GetEmployeesQuery
│   └── Tenants/
│       ├── Commands/                   # SwitchTenantCommand
│       └── Queries/                    # GetTenantsQuery
└── Strategies/                         # Country-Specific Localization Strategies
    ├── ICountryStrategyFactory.cs      # Factory resolving strategies based on CountryCode
    ├── CountryStrategyFactory.cs
    ├── ICountryInvoicingStrategy.cs
    ├── ICountryTaxAndRoundingStrategy.cs
    ├── ICountryPayrollStrategy.cs
    ├── Swiss/                          # Switzerland (CH)
    │   ├── SwissInvoicingStrategy.cs   # ISO 20022 QR-Bill generation & SCOR reference
    │   ├── SwissTaxAndRoundingStrategy.cs # 5-centimes (0.05) Swiss cash rounding
    │   └── SwissPayrollStrategy.cs     # Swissdec ELM deductions (AHV, ALV, BVG, UVG, KTG)
    └── Italian/                        # Italy (IT)
        ├── ItalianInvoicingStrategy.cs # SDI FatturaPA v1.2 XML, Split Payment, Reverse Charge
        ├── ItalianTaxAndRoundingStrategy.cs # Standard 0.01 cent rounding
        └── ItalianPayrollStrategy.cs   # INPS (9.19%), IRPEF progressive tax, TFR accrual
```

---

## ⚡ CQRS Conventions

1. **Commands:**
   - Implement `IRequest<TResult>` (where `TResult` is typically `Guid`, `bool`, or a created DTO).
   - Handlers implement `IRequestHandler<TCommand, TResult>`.
   - Handlers orchestrate changes using `IUnitOfWork` and repository methods.
2. **Queries:**
   - Implement `IRequest<TResponse>`.
   - Read-only data queries; never mutate state.
3. **Validation Pipeline:**
   - All requests automatically pass through `ValidationBehavior<TRequest, TResponse>`.
   - Any validator inheriting from `AbstractValidator<TRequest>` executes before the handler is invoked.
4. **StyleCop SA1402 Compliance:**
   - Commands, Handlers, and Validators **must** be stored in separate files.

---

## 🌐 Country Strategy Pattern

The `CountryStrategyFactory` provides runtime localization without conditional branching in business workflows:

```csharp
var strategyFactory = serviceProvider.GetRequiredService<ICountryStrategyFactory>();
var invoicingStrategy = strategyFactory.GetInvoicingStrategy(CountryCode.CH);
await invoicingStrategy.ProcessInvoiceComplianceAsync(invoice, tenant, contact);
```

### Supported Jurisdictions:
- **Switzerland (`CH`)**: CHF currency, 0.05 rounding (`Round(x * 20m) / 20m`), ISO 20022 QR-Bill data, Swissdec ELM payroll deductions.
- **Italy (`IT`)**: EUR currency, 0.01 cent rounding, SDI FatturaPA XML v1.2 metadata, INPS/IRPEF/CCNL payroll.
