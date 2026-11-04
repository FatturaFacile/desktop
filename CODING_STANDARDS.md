# FatturaFacile - Coding Standards & Best Practices

All developers and AI assistants contributing to the **FatturaFacile** repository must adhere strictly to these guidelines.

---

## 1. C# Language & Modern Idioms

- **Target Framework**: .NET 11 (`net11.0`) with C# 13 preview language features enabled.
- **Nullable Reference Types**: `<Nullable>enable</Nullable>` is enabled across all production projects. Do not suppress null warnings with `!` unless asserting invariants with an explanatory comment.
- **File-Scoped Namespaces**: Always use file-scoped namespaces (e.g., `namespace FatturaFacile.Invoicing.Domain.Aggregates;`).
- **Pattern Matching & Expressions**: Use switch expressions, property patterns, collection expressions (`[]`), and concise record syntax.
- **Primary Constructors & Init Properties**: Use primary constructors and `init` accessors for immutable DTOs and value objects.
- **Asynchronous Programming**: Use `ValueTask` / `Task`, `await using`, and avoid blocking calls (`.Result` or `.Wait()`).

---

## 2. Clean Architecture & DDD Rules

### Domain Layers (`*.Domain.Shared`, `*.Domain`)
1. **No External Dependencies**: Domain projects must never reference EF Core, ASP.NET Core, MediatR, or WPF UI libraries.
2. **Encapsulated State**:
   - Entities must provide protected/private parameterless constructors for EF Core reflection.
   - Public state changes must occur exclusively via explicit methods (e.g., `invoice.AddLine(...)`, `invoice.MarkAsPaid(...)`).
   - Collections must be exposed as `IReadOnlyCollection<T>` backed by private `List<T>` fields.
3. **Value Objects**:
   - Must inherit from `ValueObject` or be implemented as immutable records.
   - Must enforce validation invariants in their constructors.

### Application Layers (`*.Application.Shared`, `*.Application`)
1. **CQRS with MediatR**:
   - Commands mutate state and implement `ICommand<T>` or `ICommand`.
   - Queries read state and never mutate persistent data (`IQuery<T>`).
2. **DTOs & AutoMapper**:
   - Never expose domain entities directly across boundaries; project through records in `DTOs/`.
   - Map between entities and DTOs using concrete AutoMapper `Profile` classes in `Mappings/`.
3. **FluentValidation**:
   - Every command mutating persistent state must have a corresponding `AbstractValidator<TCommand>`.

### Infrastructure Layers (`*.Infrastructure.Common`, `*.Infrastructure`)
1. **EF Core**:
   - Apply entity configurations using `IEntityTypeConfiguration<T>` or clean Fluent API configurations.
   - Store value objects using EF Core `.OwnsOne()` or explicit value converters.
   - Never bypass multi-tenant global query filters unless explicitly executing cross-tenant administrative tasks (`IgnoreQueryFilters()`).
2. **Interceptors**:
   - Tenant isolation, audit timestamps, and transactional outbox event logging are handled via EF Core `SaveChangesInterceptor` implementations.

---

## 3. Desktop Presentation (WPF) Conventions

1. **ViewModels**:
   - Inherit from `ViewModelBase` using CommunityToolkit.Mvvm.
   - Use `[ObservableProperty]` and `[RelayCommand]`.
2. **Syncfusion UI Framework**:
   - Use Syncfusion specialized controls (`SfDataGrid`, `SfAccordion`, `SfNavigationDrawer`, `SfPdfViewer`).
   - Disambiguate WPF types: use `System.Windows.Controls.UserControl` and `System.Windows.Application.Current`.

---

## 4. XML Documentation Standards

In accordance with `Directory.Build.props`, `<GenerateDocumentationFile>true</GenerateDocumentationFile>` is enabled across the solution:

- Every public `class`, `struct`, `interface`, `record`, and `enum` must have an English `<summary>` docstring.
- Every public or protected `method`, `property`, `constructor`, and `event` must be documented with `<param>`, `<returns>`, and `<exception>` where applicable.
