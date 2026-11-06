# GitHub Copilot & Workspace Instructions — FatturaFacile ERP

You are acting as an expert .NET 11 software engineer and architect assisting in the development of **FatturaFacile**, an enterprise desktop ERP solution built with WPF, SQLite, and Clean Architecture.

When generating, modifying, or suggesting C# and XAML code, you **MUST ALWAYS** follow these strict project rules:

---

## 1. Zero Tactical DDD Mandate
- **NEVER** introduce tactical Domain-Driven Design patterns:
  - Do NOT generate `AggregateRoot<T>`, `Entity<T>`, or domain event raising mechanisms (`RaiseDomainEvent`, `AddDomainEvent`).
  - Do NOT implement an Outbox pattern or outbox messages table.
  - Do NOT create abstract `ValueObject` classes using reflection for equality. Use lightweight C# positional records (e.g., `public sealed record Money(decimal Amount, string Currency);`).
  - Do NOT use private setters or private parameterless constructors on entities.
- **ALWAYS** model domain entities as clean, pragmatic POCOs with:
  - Public parameterless constructor (or default auto-property initializers).
  - Public auto-properties with `{ get; set; }`.
  - Appropriate multi-tenant interface (`ITenantScopedEntity` or `IOptionalTenantEntity`).

---

## 2. C# 13/14 & Language Features
- Target framework is **.NET 11** (`net11.0`).
- Nullable reference types are enabled (`<Nullable>enable</Nullable>`). Always handle potential nulls cleanly.
- Use primary constructors where appropriate for DI-injected services.
- Use pattern matching, collection expressions (`[...]`), and raw string literals.
- Ensure all asynchronous methods accept a `CancellationToken cancellationToken = default` where appropriate.

---

## 3. StyleCop & Analyzer Compliance
- **Mandatory Copyright Header:** Every `.cs` file MUST start with the StyleCop header:
  ```csharp
  // <copyright file="<ExactFileName>.cs" company="Fattura Facile">
  // Copyright (c) Fattura Facile. All rights reserved.
  // </copyright>
  ```
- **One Type Per File (SA1402):** Never place multiple classes, interfaces, or records in the same file. Separate Commands, Handlers, and Validators into their own dedicated files.
- **Mandatory XML Comments (CS1591 / SA1600):** Every public class, struct, record, interface, enum, method, constructor, and property MUST have complete XML documentation in English (`<summary>`, `<param>`, `<returns>`).

---

## 4. Pure MVVM with CommunityToolkit.Mvvm
- **No Code-Behind Event Handlers:** Never generate UI event handlers (`Button_Click`, `SelectionChanged`, `Loaded`) in `*.xaml.cs` files. WPF code-behind files must contain ONLY `InitializeComponent()`.
- Use CommunityToolkit.Mvvm source generators:
  - `[ObservableProperty]` on private fields (e.g. `_name` generates `Name` property with change notification).
  - `[RelayCommand]` on methods to generate async/sync `ICommand` properties.
- Use `ISender` (MediatR) to dispatch requests from ViewModels to Application handlers.

---

## 5. Multi-Tenancy & Security Rules
- **Tenant Isolation:** Entities implementing `ITenantScopedEntity` must have non-nullable `Guid TenantId { get; set; }`.
- **Shared Defaults:** Entities implementing `IOptionalTenantEntity` have nullable `Guid? TenantId { get; set; }`. When `TenantId == null`, the entity is a global read-only template shared across all tenants.
- **Never Modify Global Records:** Attempting to update or delete a record with `TenantId == null` throws `InvalidOperationException`.
- **Windows Hello:** Biometric security is enforced in `App.xaml.cs` using WinRT `UserConsentVerifier`.

---

## 6. Country Localization (CH vs IT)
- Implement regional logic via `ICountryStrategyFactory`:
  - **Switzerland (`CH`)**: CHF currency, Swiss 0.05 cash rounding (`Round(x * 20m) / 20m`), ISO 20022 QR-Bills (`InvoiceQrBillData`), Swissdec ELM payroll.
  - **Italy (`IT`)**: EUR currency, standard 0.01 cent rounding, SDI FatturaPA v1.2 XML metadata (`InvoiceSdiMetadata`), INPS/IRPEF/CCNL payroll.
