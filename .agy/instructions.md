Before anything else read `.ai/rules/index.md` and the rule files it maps to your paths (always `.ai/rules/project.md`).

# agy Operational Instructions — FatturaFacile ERP Enterprise

> [!IMPORTANT]
> This file is actively read by the Antigravity CLI (`agy`) agent during CLI executions and automated workflows.

## System Mission & Architectural Directives
You are operating within **FatturaFacile**, a .NET 11 desktop ERP solution (WPF, SQLite, Syncfusion).
Follow the operational guidelines defined below:

1. **NO TACTICAL DDD:**
   - Tactical DDD is strictly prohibited. Do NOT use Aggregate Roots, Domain Events, or Outbox tables.
   - Entities are pragmatic POCOs with public parameterless constructors and direct `{ get; set; }` properties.
2. **LAYER BOUNDARIES:**
   - `Domain` -> `Application` -> `Infrastructure` -> `Wpf`. Unidirectional dependencies only.
3. **MULTI-TENANCY:**
   - Non-nullable `TenantId` on `ITenantScopedEntity`.
   - Nullable `TenantId` on `IOptionalTenantEntity` (`TenantId == null` is a read-only global default).
   - Global modifications on `TenantId == null` throw `InvalidOperationException`.
4. **COUNTRY STRATEGIES:**
   - Switzerland (`CH`): CHF, Swiss 5-centimes (0.05) rounding, ISO 20022 QR-Bills, Swissdec ELM payroll deductions.
   - Italy (`IT`): EUR, 0.01 rounding, SDI / FatturaPA v1.2 XML metadata, INPS / IRPEF / TFR / CCNL payroll.
5. **MVVM & SECURITY:**
   - Pure MVVM with CommunityToolkit.Mvvm.
   - Zero code-behind in WPF (`*.xaml.cs` has only `InitializeComponent()`).
   - Windows Hello biometric / PIN gate in `App.xaml.cs`.
6. **CODE QUALITY & STYLECOP:**
   - Mandatory copyright header: `// <copyright file="..." company="Fattura Facile">`
   - Mandatory XML comments on all public members.
   - Strictly one type per file (SA1402).

For detailed routing tables and feature implementation recipes, refer to [AGENTS.md](../AGENTS.md).
