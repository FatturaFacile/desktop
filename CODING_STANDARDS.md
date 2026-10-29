# FatturaFacile - Coding Standards & Best Practices

All developers and AI assistants contributing to the **FatturaFacile** repository must adhere strictly to these guidelines.

---

## 1. C# Language & Modern Idioms

- **Target Framework**: .NET 11 with C# 13 preview language features enabled.
- **Nullable Reference Types**: `<Nullable>enable</Nullable>` is enabled across all production projects. Do not suppress null warnings with `!` unless asserting invariants with an explanatory comment. (Legacy sample libraries may have nullable disabled to prevent external warning-as-error escalations).
- **File-Scoped Namespaces**: Always use file-scoped namespaces (e.g., `namespace FatturaFacile.Domain.Entities;`).
- **Pattern Matching & Expressions**: Use switch expressions, property patterns, collection expressions (`[]`), and concise record syntax where appropriate.
- **Primary Constructors & Init Properties**: Use primary constructors and `init` accessors for immutable DTOs and value objects.
- **Asynchronous Programming**: Use `ValueTask` / `Task`, `await using`, and avoid `.Result` or `.Wait()`.

---

## 2. Clean Architecture & DDD Rules

### Domain Layer (`FatturaFacile.Domain`)
1. **No External Dependencies**: Domain must never reference EF Core, ASP.NET Core, MediatR, or WPF UI libraries.
2. **Encapsulated State**:
   - Entities must provide private parameterless constructors for EF Core reflection.
   - Public state changes must occur exclusively via explicit factory methods or behavior-driven public methods (e.g., `client.UpdateAddress(...)`, `invoice.AddLine(...)`).
   - Collections must be exposed as `IReadOnlyCollection<T>` backed by private `List<T>` fields.
3. **Value Objects**:
   - Must inherit from `ValueObject` and implement `GetEqualityComponents()`.
   - Must enforce validation invariants in their constructors.
   - Must be immutable.

### Application Layer (`FatturaFacile.Application`)
1. **CQRS with MediatR**:
   - Commands mutate state and implement `IRequest<T>` or `IRequest`.
   - Queries read state and never mutate persistent data.
   - Maintain one command/query per file alongside its corresponding handler and validator.
2. **DTOs & AutoMapper**:
   - Never expose domain entities directly to the presentation layer; project through records in `DTOs/`.
   - Map between entities and DTOs using concrete AutoMapper `Profile` classes in `Mappings/`.
3. **FluentValidation**:
   - Every command mutating persistent state must have a corresponding `AbstractValidator<TCommand>`.

### Infrastructure Layer (`FatturaFacile.Infrastructure`)
1. **EF Core**:
   - Apply entity configurations using `IEntityTypeConfiguration<T>` or clean Fluent API inside `FatturaFacileDbContext`.
   - Store value objects using EF Core `.OwnsOne()` or explicit value converters.
   - Never bypass multi-tenant global query filters unless explicitly executing cross-tenant administrative tasks (`IgnoreQueryFilters()`).
2. **Exporters & Cross-Border Parsers**:
   - Keep XML/EDI serialization decoupled from database persistence.
   - Exporters generate standards-compliant XML (FatturaPA 1.2.2, ISO 20022 `pain.001`, `camt.053`) and SVG/PDF formats (Swiss QR-Bill).

---

## 3. Modular Presentation & Syncfusion MVVM Conventions

### Architectural Separation
- Presentation modules (`FatturaFacile.Wpf.Modules`, `FatturaFacile.Wpf.Reporting`, `FatturaFacile.Wpf.AI`) **must never** reference `FatturaFacile.Infrastructure`.
- Modules depend strictly on `FatturaFacile.Application` and `FatturaFacile.Wpf.Common`.
- The entry point shell (`FatturaFacile.csproj`) acts as the composition root, registering Infrastructure and Presentation modules into the application host.

### Module Registration (`IModuleInitializer`)
- Each module implements `IModuleInitializer` to register its Views, ViewModels, and internal services into `IServiceCollection`.
- ViewModels are registered with `Transient` lifetime and resolved on demand via `IViewModelFactory`.

### MVVM & Syncfusion UI Framework
1. **ViewModels**:
   - Inherit from `ViewModelBase` or `DocumentViewModelBase`.
   - Use `SetProperty(ref field, value)` or CommunityToolkit.Mvvm observable properties.
   - Use `RelayCommand` or `AsyncRelayCommand` for actions and asynchronous operations.
   - Implement `IDocumentModule` (`ModuleId`, `Caption`, `IsActive`) for dockable document tabs.
2. **Decoupled Dialogs & UI Services**:
   - Never call `MessageBox.Show` or instantiate modal windows directly in ViewModels.
   - Use abstracted UI contracts (`IDialogService`, `IMessageBoxService`, `INotificationService`) resolved through dependency injection for testability.
3. **Views & XAML**:
   - Use dynamic `{loc:Translate KeyName, Default='Fallback'}` markup extension for all user-facing text.
   - Apply Syncfusion themes via `SfSkinManager` (e.g., `FluentDark`, `FluentLight`, `Material3`) applied at root or element level.
   - Use Syncfusion specialized controls (`SfDataGrid`, `SfTreeGrid`, `SfChart`, `SfScheduler`, `SfTextInputLayout`, `ChromelessWindow`).
   - Persist dock state using Syncfusion `DockingManager` serialization methods (`SaveDockState` / `LoadDockState`).
   - Disambiguate WPF types: use `System.Windows.Controls.UserControl` and `System.Windows.Application.Current`.
4. **License Registration**:
   - Syncfusion license validation must occur strictly in the composition root (`App.xaml.cs`) prior to initializing UI components:
     ```csharp
     Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionKey);
     ```

### Windows Store (MSIX) Compatibility
- The application installation folder (`C:\Program Files\WindowsApps\...`) is strictly read-only.
- Never write databases, caches, or state files to `AppDomain.CurrentDomain.BaseDirectory`.
- Persist all mutable files (`FatturaFacile.db`, `userstate.json`, layout XMLs) in `Environment.SpecialFolder.LocalApplicationData` (`%LocalAppData%\FatturaFacile\`).

---

## 4. XML Documentation Standards

In accordance with `Directory.Build.props`, `<GenerateDocumentationFile>true</GenerateDocumentationFile>` is enabled across the solution:

- Every public `class`, `struct`, `interface`, `record`, and `enum` must have an English `<summary>` docstring.
- Every public or protected `method`, `property`, `constructor`, and `event` must be documented with `<param>`, `<returns>`, and `<exception>` where applicable.
- ViewModel commands and lifecycle methods (`InitializeAsync`, `OnClose`) must document their behavior and UI side effects.

---

## 5. Cross-Border Naming & Formats

- **Monetary Values**: Always use the `Money` value object containing both `Amount` and `Currency` (EUR or CHF). Prohibit cross-currency math.
- **Swiss Cash Rounding**: Apply `ToSwissCashRounding()` for CHF cash transactions to ensure statutory 0.05 rounding.
- **Tax Identifiers**:
  - Italy: `FiscalCode` (Codice Fiscale) and `VatNumber` (Partita IVA).
  - Switzerland: `SwissUid` (`CHE-xxx.xxx.xxx`) and cantonal abbreviations (e.g., `TI`, `ZH`, `GE`).
- **Electronic Formats**:
  - Italy: SDI FatturaPA XML v1.2.2.
  - Switzerland: Swiss QR-Bill specification with 27-digit SCOR Modulo 10 reference.

---

## 6. Automated Testing Guidelines

The solution maintains two distinct test projects:

### 1. `FatturaFacile.Application.UnitTests` (xUnit)
- Primary framework: **xUnit** (`[Fact]`, `[Theory]`).
- Assertion library: **FluentAssertions** (`result.Should().Be(...)`).
- Mocking: **Moq** / **NSubstitute** for application interfaces.
- Scope: Domain entities, invariants, value objects, MediatR command/query handlers, pipeline behaviors, AutoMapper profiles, infrastructure security/hashers.

### 2. `FatturaFacile.Tests` (NUnit WPF Functional Tests)
- Primary framework: **NUnit** (`[Test]`, `[SetUp]`).
- Base fixture: `BaseTestFixture` hosting an in-process WPF `System.Windows.Application`, Syncfusion theme resources, and `DispatcherHelper.DoEvents()` to pump the STA dispatcher queue.
- Scope: Presentation navigation, module registration, Syncfusion `DockingManager` serialization, document activation states.
