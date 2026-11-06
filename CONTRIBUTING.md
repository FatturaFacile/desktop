# Contributing to FatturaFacile ERP 🚀

Thank you for your interest in contributing to **FatturaFacile**! This document serves as the official developer onboarding guide and engineering handbook for all developers—from junior engineers to principal architects—as well as autonomous AI coding agents.

Please read this document carefully before opening pull requests or contributing code.

---

## 🧭 Core Principles & Architecture Philosophy

1. **Strict Clean Architecture:**
   Dependencies point inward: `Domain <- Application <- Infrastructure / Wpf`. Never bypass a layer.
2. **ZERO Tactical DDD Mandate:**
   Tactical Domain-Driven Design (AggregateRoots, Domain Events, Outbox patterns, reflection-based ValueObjects) is **PROHIBITED**. Entities are clean, pragmatic POCOs with public parameterless constructors and direct property accessors (`{ get; set; }`).
3. **Pure MVVM with Zero Code-Behind:**
   The presentation layer strictly utilizes `CommunityToolkit.Mvvm`. View code-behind files (`*.xaml.cs`) must contain only `InitializeComponent()`. All business logic resides in Application CQRS handlers.
4. **Strict Analyzer Enforcement:**
   Warnings are treated as errors (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`). Unformatted code, missing XML comments, or missing copyright headers will fail the build immediately.

---

## 💻 Development Environment Setup

### Required Tooling
- **Operating System:** Windows 10 (Build 19041+) or Windows 11 (required for WinRT Windows Hello biometric APIs).
- **.NET SDK:** .NET 11 SDK (version specified in `global.json`, e.g. `11.0.100-rc.3`).
- **IDE:**
  - Visual Studio 2026 / Visual Studio 2022 (v17.12+) with `.NET desktop development` and `Universal Windows Platform development` workloads.
  - JetBrains Rider 2024.3+ with .NET 11 support.
  - VS Code with the official C# Dev Kit extension.
- **Git:** Configured with CRLF line endings for Windows (`git config --global core.autocrlf true`).

### Cloning and Restoring
```powershell
git clone https://github.com/FatturaFacile/desktop.git
cd desktop
dotnet restore
```

---

## 📐 Coding Standards & Quality Gates

### 1. Mandatory File Copyright Headers (StyleCop `SA1633`)
Every `.cs` file in the repository **must** start with the standardized copyright header:

```csharp
// <copyright file="Invoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>
```
> [!WARNING]
> Replace `"Invoice.cs"` with the exact filename. Mismatches or omitted headers trigger compiler error `SA1633`.

### 2. Mandatory XML Documentation (Rules `CS1591` & `SA1600`)
Every public class, record, interface, enum, struct, constructor, method, property, and field must have complete, meaningful XML documentation in English:

```csharp
/// <summary>
/// Calculates the gross total and applies country-specific tax and rounding rules.
/// </summary>
/// <param name="subtotal">The net taxable subtotal.</param>
/// <param name="vatRate">The applicable VAT percentage rate.</param>
/// <returns>The computed <see cref="Money"/> object containing rounded gross amount.</returns>
public Money CalculateGrossAmount(decimal subtotal, decimal vatRate)
{
    // ...
}
```

### 3. Single Type Per File Rule (StyleCop `SA1402`)
Each `.cs` file must contain **exactly one** top-level type (one class, interface, enum, or record).
- ❌ **Do NOT** place a Command and its Handler or Validator in the same file.
- ✅ **DO** split them into separate files:
  - `CreateInvoiceCommand.cs`
  - `CreateInvoiceCommandHandler.cs`
  - `CreateInvoiceCommandValidator.cs`

### 4. Pragmatic Clean POCO Guidelines (No DDD)
When adding or updating domain models in `FatturaFacile.Domain`:
- Always provide a public parameterless constructor (or default auto-property initializers).
- Use auto-properties with `{ get; set; }`.
- Do **not** inherit from `Entity<T>` or `AggregateRoot<T>`.
- Do **not** implement domain events, event dispatchers, or transactional outbox records.
- Implement the appropriate multi-tenancy interface:
  - `ITenantScopedEntity` for tenant-private records (mandatory `Guid TenantId { get; set; }`).
  - `IOptionalTenantEntity` for shared/global defaults (nullable `Guid? TenantId { get; set; }`).

```csharp
// <copyright file="Product.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents a catalog product or service item.
/// </summary>
public class Product : IOptionalTenantEntity
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the optional owning tenant identifier. If null, the product is a global template.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the unique product SKU code.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selling price.
    /// </summary>
    public decimal SalePrice { get; set; }
}
```

---

## ⚡ CQRS & MediatR Conventions

All business operations in `FatturaFacile.Application` follow CQRS with **MediatR**:

### 1. Commands & Queries
- Commands represent side-effects (mutations). Return simple types (e.g., `Guid`, `bool`) or a DTO.
- Queries represent read-only operations. Never mutate database state.

### 2. Validation with FluentValidation
Every command or query with input arguments should have a corresponding `AbstractValidator<TRequest>`. The `ValidationBehavior<TRequest, TResponse>` pipeline behavior automatically validates incoming requests before the handler executes:

```csharp
// <copyright file="CreateContactCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.Contacts.Commands;

/// <summary>
/// Validator for <see cref="CreateContactCommand"/>.
/// </summary>
public sealed class CreateContactCommandValidator : AbstractValidator<CreateContactCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateContactCommandValidator"/> class.
    /// </summary>
    public CreateContactCommandValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .MaximumLength(255)
            .WithMessage("Company name is required.");

        RuleFor(x => x.Country)
            .NotEmpty()
            .Length(2)
            .WithMessage("Country must be a valid 2-letter ISO code (e.g., CH or IT).");
    }
}
```

### 3. AutoMapper Rules
- All mappings between Domain entities and Application DTOs are configured in `FatturaFacile.Application.Common.Mappings.MappingProfile`.
- Every property mapping must be deterministic.
- Whenever you add a new DTO or Entity mapping, execute the unit test `AutoMapperConfigurationTests.cs`:
  ```csharp
  mapper.ConfigurationProvider.AssertConfigurationIsValid();
  ```
  This ensures no unmapped properties break runtime serialization.

---

## 🖥️ Presentation Layer & Pure MVVM Guidelines

The WPF presentation layer (`FatturaFacile.Wpf`) strictly implements pure MVVM:

### 1. ViewModels with CommunityToolkit.Mvvm
- Inherit from `ObservableObject`.
- Use `[ObservableProperty]` on private fields (e.g. `_name` generates `Name` property with `OnPropertyChanged`).
- Use `[RelayCommand]` on methods to generate async/sync `ICommand` bindings.
- Inject `ISender` for MediatR interactions and UI service abstractions (e.g., `IDocumentExportService`).

```csharp
[ObservableProperty]
private ObservableCollection<InvoiceDto> _invoices = new();

[RelayCommand]
public async Task LoadInvoicesAsync()
{
    var result = await _sender.Send(new GetInvoicesQuery());
    Invoices = new ObservableCollection<InvoiceDto>(result);
}
```

### 2. Zero Code-Behind Mandate
- No UI event handlers (`Button_Click`, `Grid_SelectionChanged`, etc.) are permitted in `*.xaml.cs`.
- Use XAML command bindings (`Command="{Binding IssueInvoiceCommand}"`) or attached behaviors.
- Views are registered as Singletons or Transients in `App.xaml.cs` via Microsoft Dependency Injection.

---

## 🧪 Testing Guidelines

All unit and integration tests reside in `tests/FatturaFacile.UnitTests`:

### 1. Multi-Tenant Isolation Testing
When writing tests for data access or business logic:
- Mock `ICurrentTenantService` to simulate active tenant switches.
- Verify that tenant A cannot read or mutate tenant B's records.
- Verify that modifying a global default (`TenantId == null`) throws `InvalidOperationException`.

### 2. Country Strategy Verification
- Swiss calculations must test the 0.05 cash rounding boundary (`10.02 -> 10.00`, `10.03 -> 10.05`, `10.07 -> 10.05`, `10.08 -> 10.10`).
- Swiss QR-bill data must pass `SwissQrBillPayloadBuilder.Validate` (QRR only with QR-IBAN, SCOR/NON with regular IBAN, structured addresses).
- Italian SDI tests must validate XML payload completeness (Codice Destinatario, Split Payment, Natura IVA).

### 3. Architecture Gate Tests
Do not introduce direct dependencies between `Domain` and `Infrastructure` or `Domain` and `Wpf`. Architectural integrity tests verify dependency directions and prevent leakage.

---

## 🔄 Git Workflow & Pull Request Process

1. **Branch Naming:**
   - Features: `feature/<short-description>`
   - Bugfixes: `hotfix/<short-description>`
   - Documentation: `docs/<short-description>`
2. **Pre-Commit Checklist:**
   Before pushing your commits, run the following commands locally:
   ```powershell
   # 1. Format code according to .editorconfig
   dotnet format

   # 2. Build in Release mode with zero warnings/errors
   dotnet build --configuration Release

   # 3. Run all tests
   dotnet test --configuration Release --no-build
   ```
3. **Commit Messages:**
   Write clear, imperative commit messages in English (e.g. `feat(invoicing): implement Swissdec ELM payroll deductions`).
4. **Pull Request Quality Review:**
   - Ensure all CI workflow checks pass (`.github/workflows/ci.yml`).
   - PR description must summarize the change, link relevant issues, and confirm no tactical DDD patterns were introduced.

Thank you for helping keep FatturaFacile clean, reliable, and blazing fast!
