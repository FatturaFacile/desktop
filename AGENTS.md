# AGENTS.md — Autonomous Agent Operational Guide
## FatturaFacile ERP Enterprise (.NET 11 / WPF / SQLite)

> [!IMPORTANT]
> **MANDATORY READING FOR ALL AI CODING AGENTS (Antigravity CLI / agy, Copilot, Gemini, Claude, Cursor)**
> This document specifies strict architectural rules, file routing tables, and recipes for extending FatturaFacile. You MUST comply with every invariant listed below.

---

## ⚡ Core Architectural Invariants (Agent Rules)

1. **ZERO DDD MANDATE (PROHIBITED PATTERNS):**
   - 🚫 **NEVER** create `AggregateRoot`, `Entity<TId>`, or domain event raising mechanisms.
   - 🚫 **NEVER** create an outbox table, outbox interceptor, or background worker event dispatcher.
   - 🚫 **NEVER** create reflection-based `ValueObject` classes. Use C# `record` (e.g. `Money`).
   - 🚫 **NEVER** use private setters on domain entities. All entity properties MUST be public `{ get; set; }` with a public parameterless constructor.

2. **CLEAN ARCHITECTURE DEPENDENCY INVERSION:**
   - `Domain` has **zero** package references.
   - `Application` depends **only** on `Domain`.
   - `Infrastructure` implements interfaces defined in `Application`.
   - `Wpf` depends on `Application` and `Infrastructure` for DI registration.

3. **PURE MVVM (ZERO CODE-BEHIND):**
   - 🚫 **NEVER** write event handlers (e.g., `Button_Click`, `Loaded`, `SelectionChanged`) in `*.xaml.cs` files.
   - All code-behind files must contain ONLY `InitializeComponent()`.
   - ViewModels use `[ObservableProperty]` and `[RelayCommand]` from `CommunityToolkit.Mvvm`.
   - ViewModels interact with the business layer exclusively via MediatR (`ISender`).

4. **STYLECOP & ANALYZER COMPLIANCE:**
   - 🚫 **NEVER** place multiple types in one file (StyleCop `SA1402`). Strictly **one type per file**.
   - Every file MUST begin with the exact copyright header:
     ```csharp
     // <copyright file="<FileName>.cs" company="Fattura Facile">
     // Copyright (c) Fattura Facile. All rights reserved.
     // </copyright>
     ```
   - Every public type and member MUST have complete XML documentation (`<summary>`, `<param>`, `<returns>`).

5. **MULTI-TENANT ENFORCEMENT:**
   - Tenant-private records MUST implement `ITenantScopedEntity` (mandatory non-nullable `Guid TenantId { get; set; }`).
   - Shared/global defaults MUST implement `IOptionalTenantEntity` (`Guid? TenantId { get; set; }`). When `TenantId == null`, records are strictly read-only and shared across all tenants.
   - 🚫 **NEVER** bypass `TenantSaveChangesInterceptor`.

---

## 🗺️ File Path Routing Table

| Concern | Target Directory | Naming Convention / File Format | Example |
| :--- | :--- | :--- | :--- |
| **Domain Entities** | `src/FatturaFacile.Domain/Entities/` | `<EntityName>.cs` | `Invoice.cs`, `Contact.cs` |
| **Domain Enums** | `src/FatturaFacile.Domain/Enums/` | `<EnumName>.cs` | `CountryCode.cs`, `InvoiceStatus.cs` |
| **Domain Primitives** | `src/FatturaFacile.Domain/Common/` | `<Name>.cs` | `Money.cs`, `ITenantScopedEntity.cs` |
| **CQRS Commands** | `src/FatturaFacile.Application/Features/<Feature>/Commands/` | `<Action><Entity>Command.cs` | `CreateInvoiceCommand.cs` |
| **CQRS Handlers** | `src/FatturaFacile.Application/Features/<Feature>/Commands/` | `<Action><Entity>CommandHandler.cs` | `CreateInvoiceCommandHandler.cs` |
| **CQRS Validators** | `src/FatturaFacile.Application/Features/<Feature>/Commands/` | `<Action><Entity>CommandValidator.cs` | `CreateInvoiceCommandValidator.cs` |
| **CQRS Queries** | `src/FatturaFacile.Application/Features/<Feature>/Queries/` | `Get<Entity>Query.cs` | `GetInvoicesQuery.cs` |
| **DTOs** | `src/FatturaFacile.Application/DTOs/` | `<Entity>Dto.cs` | `InvoiceDto.cs`, `ContactDto.cs` |
| **AutoMapper Mappings** | `src/FatturaFacile.Application/Common/Mappings/` | `MappingProfile.cs` | `MappingProfile.cs` |
| **Country Strategies** | `src/FatturaFacile.Application/Strategies/<Country>/` | `<Country><Domain>Strategy.cs` | `SwissInvoicingStrategy.cs` |
| **EF Core DbContext** | `src/FatturaFacile.Infrastructure/Persistence/` | `FatturaFacileDbContext.cs` | `FatturaFacileDbContext.cs` |
| **Repositories** | `src/FatturaFacile.Infrastructure/Repositories/` | `Repository.cs`, `UnitOfWork.cs` | `UnitOfWork.cs` |
| **Infrastructure Services** | `src/FatturaFacile.Infrastructure/Services/` | `<Service>Service.cs` | `WindowsHelloAuthenticationService.cs` |
| **WPF ViewModels** | `src/FatturaFacile.Wpf/ViewModels/` | `<Feature>ViewModel.cs` | `InvoicingViewModel.cs` |
| **WPF Views (XAML)** | `src/FatturaFacile.Wpf/Views/` | `<Feature>View.xaml` | `InvoicingView.xaml` |
| **WPF Code-Behind** | `src/FatturaFacile.Wpf/Views/` | `<Feature>View.xaml.cs` | `InvoicingView.xaml.cs` |
| **Unit Tests** | `tests/FatturaFacile.UnitTests/` | `<Subject>Tests.cs` | `CountryStrategyTests.cs` |

---

## 🛠️ Step-by-Step Feature Implementation Recipe

Follow this exact recipe when adding a new business feature or entity (e.g., `DeliveryNote`):

### Step 1: Create Domain Entity (`FatturaFacile.Domain`)
File: `src/FatturaFacile.Domain/Entities/DeliveryNote.cs`
- Include StyleCop header.
- Implement `ITenantScopedEntity` (or `IOptionalTenantEntity`).
- Provide public parameterless constructor and `{ get; set; }` properties.
- Add XML documentation.

### Step 2: Create DTO (`FatturaFacile.Application`)
File: `src/FatturaFacile.Application/DTOs/DeliveryNoteDto.cs`
- Pure C# record or class representing the UI projection.

### Step 3: Register in AutoMapper Profile
File: `src/FatturaFacile.Application/Common/Mappings/MappingProfile.cs`
- Add mapping: `CreateMap<DeliveryNote, DeliveryNoteDto>().ReverseMap();`

### Step 4: Create CQRS Command, Handler, and Validator
- `CreateDeliveryNoteCommand.cs` (`IRequest<Guid>`)
- `CreateDeliveryNoteCommandHandler.cs` (`IRequestHandler<CreateDeliveryNoteCommand, Guid>`)
- `CreateDeliveryNoteCommandValidator.cs` (`AbstractValidator<CreateDeliveryNoteCommand>`)
- Note: Keep each in a separate file (Rule `SA1402`).

### Step 5: Configure EF Core Mapping (`FatturaFacile.Infrastructure`)
File: `src/FatturaFacile.Infrastructure/Persistence/FatturaFacileDbContext.cs`
- Add `public DbSet<DeliveryNote> DeliveryNotes => Set<DeliveryNote>();`
- In `OnModelCreating`, configure table name, column names, keys, and global query filter:
  ```csharp
  b.HasQueryFilter(e => e.TenantId == _currentTenantService.TenantId);
  ```

### Step 6: Create WPF ViewModel (`FatturaFacile.Wpf`)
File: `src/FatturaFacile.Wpf/ViewModels/DeliveryNotesViewModel.cs`
- Decorate class with `partial`.
- Inject `ISender`.
- Add `[ObservableProperty] private ObservableCollection<DeliveryNoteDto> _deliveryNotes = new();`
- Add `[RelayCommand] private async Task LoadDeliveryNotesAsync() { ... }`

### Step 7: Create WPF View (`FatturaFacile.Wpf`)
Files: `DeliveryNotesView.xaml` and `DeliveryNotesView.xaml.cs`
- XAML: Bind `ItemsSource="{Binding DeliveryNotes}"` inside a `syncfusion:SfDataGrid`.
- Code-behind: Only `InitializeComponent()`.

### Step 8: Register in DI (`App.xaml.cs`)
File: `src/FatturaFacile.Wpf/App.xaml.cs`
- Register `services.AddSingleton<DeliveryNotesViewModel>();`
- In `MainWindow.xaml`, add a tab/navigation button and `DataTemplate` mapping.

### Step 9: Add Unit Test
File: `tests/FatturaFacile.UnitTests/DeliveryNotesTests.cs`
- Validate tenant isolation and command execution.

---

## 💻 CLI Commands & Diagnostics

### Build and Format
```powershell
# Restore dependencies
dotnet restore

# Format code (must pass in CI)
dotnet format --verify-no-changes --verbosity diagnostic

# Build in Release configuration
dotnet build --configuration Release --no-restore
```

### Run Tests
```powershell
# Run all unit tests
dotnet test --configuration Release --no-build --logger trx
```

### Run Application
```powershell
# Run the WPF client
dotnet run --project src/FatturaFacile.Wpf/FatturaFacile.Wpf.csproj
```

---

## 🚫 Agent "Dos and Don'ts" Checklist

| Do | Don't |
| :--- | :--- |
| ✅ Use `[ObservableProperty]` and `[RelayCommand]` from CommunityToolkit.Mvvm | ❌ Implement `INotifyPropertyChanged` manually |
| ✅ Use clean POCOs with public `{ get; set; }` | ❌ Use `AggregateRoot` or private setters |
| ✅ Use `Money` record for monetary representations | ❌ Create custom reflection-based ValueObject hierarchies |
| ✅ Split Command, Handler, and Validator into separate files | ❌ Combine multiple types in a single file (`SA1402`) |
| ✅ Include full XML documentation on all public members | ❌ Omit XML documentation (`CS1591` / `SA1600`) |
| ✅ Include StyleCop SA1633 copyright header on every file | ❌ Leave files without copyright headers |
| ✅ Keep WPF code-behind files strictly at `InitializeComponent()` | ❌ Add UI event handlers in `.xaml.cs` |
| ✅ Use `_sender.Send(...)` to execute commands and queries | ❌ Access DbContext directly from ViewModels |
