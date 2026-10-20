# FatturaFacile - Coding Standards & Best Practices

All developers and AI assistants contributing to the **FatturaFacile** repository must adhere strictly to these guidelines.

---

## 1. C# Language & Modern Idioms

- **Target Framework**: .NET 9 / .NET 11 with C# 13 language features enabled.
- **Nullable Reference Types**: `<Nullable>enable</Nullable>` is enabled across all projects. Do not suppress null warnings with `!` unless asserting invariants with an explanatory comment.
- **File-Scoped Namespaces**: Always use file-scoped namespaces (`namespace FatturaFacile.Domain.Entities;`).
- **Pattern Matching & Expressions**: Use switch expressions, property patterns, and concise record syntax where appropriate.
- **Primary Constructors & Init Properties**: Use primary constructors and `init` accessors for immutable DTOs and value objects.

---

## 2. Clean Architecture & DDD Rules

### Domain Layer (`FatturaFacile.Domain`)
1. **No External Dependencies**: Domain must never reference EF Core, ASP.NET Core, MediatR, or WPF libraries.
2. **Encapsulated State**:
   - Entities must have private parameterless constructors for EF Core reflection.
   - Public state changes must occur exclusively via explicit factory methods or behavior-driven public methods (e.g., `client.UpdateAddress(...)`, `invoice.AddLine(...)`).
   - Collections must be exposed as `IReadOnlyCollection<T>` backed by private `List<T>` fields.
3. **Value Objects**:
   - Must inherit from `ValueObject` and implement `GetEqualityComponents()`.
   - Must enforce validation invariants in their constructors.
   - Must be immutable.

### Application Layer (`FatturaFacile.Application`)
1. **CQRS with MediatR**:
   - Commands mutate state and implement `IRequest<T>` or `IRequest`.
   - Queries read state and never mutate the database.
   - One command/query per file alongside its corresponding handler and validator.
2. **DTOs & AutoMapper**:
   - Never expose domain entities directly to the presentation layer. Use records in `DTOs/`.
   - Map between entities and DTOs using concrete AutoMapper `Profile` classes in `Mappings/`.
3. **FluentValidation**:
   - Every command mutating persistent state must have a corresponding `AbstractValidator<TCommand>`.

### Infrastructure Layer (`FatturaFacile.Infrastructure`)
1. **EF Core**:
   - Apply entity configurations using `IEntityTypeConfiguration<T>` or clear Fluent API inside `FatturaFacileDbContext`.
   - Store value objects using EF Core `.OwnsOne()` or explicit value converters.
   - Never bypass multi-tenant global query filters unless explicitly implementing cross-tenant administrative tasks (`IgnoreQueryFilters()`).

---

## 3. WPF & MVVM Conventions (`CommunityToolkit.Mvvm`)

1. **ViewModels**:
   - ViewModels must inherit from `ViewModelBase` (derived from `ObservableObject`).
   - Use `[ObservableProperty]` source generators for reactive properties.
   - Use `[RelayCommand]` for actions and asynchronous operations (`async Task ExecuteAsync()`).
   - Guard against multiple simultaneous async triggers using `IsBusy` flags or command execution states.
2. **Views & XAML**:
   - Views must use the dynamic `{loc:Translate KeyName, Default='Fallback'}` markup extension for all user-facing labels.
   - Styles, colors, and brushes must reference theme keys (`{DynamicResource SurfaceBrush}`, `{DynamicResource TextPrimaryBrush}`) to support seamless light/dark theme switching.
   - Avoid code-behind logic in views unless strictly managing UI-only visual elements (e.g., password box binding or window drag).

---

## 4. Cross-Border Naming & Formats

- **Monetary Values**: Always use the `Money` value object containing both `Amount` and `Currency` (EUR or CHF).
- **Tax Identifiers**:
  - Italy: `FiscalCode` (Codice Fiscale) and `VatNumber` (Partita IVA).
  - Switzerland: `SwissUid` (`CHE-xxx.xxx.xxx`) and cantonal abbreviations (e.g., `TI`, `ZH`, `GE`).
- **Electronic Formats**:
  - Italy: SDI FatturaPA XML v1.2.2.
  - Switzerland: Swiss QR-Bill specification with 27-digit SCOR Modulo 10 reference.

---

## 5. Automated Testing Guidelines

- **Unit Testing Framework**: xUnit (`[Fact]`, `[Theory]`).
- **Assertion Library**: `FluentAssertions` (`result.Should().Be(...)`).
- **Mocking**: `Moq` for application interfaces.
- **Test Organization**:
  - Test classes should mirror the namespace of the subject under test.
  - Test method naming convention: `UnitOfWork_StateUnderTest_ExpectedBehavior`.
