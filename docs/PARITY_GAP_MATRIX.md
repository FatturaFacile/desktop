# web-api ↔ Desktop Parity Gap Matrix

Baseline: desktop `dotnet build -c Release` = 0 warnings / 0 errors.
web-api: 65 modules, ~600 controllers, ~3,150 OpenAPI paths (`docs/api/openapi.json`).

## Cross-cutting infrastructure

| Capability | Desktop state | Gap |
| :-- | :-- | :-- |
| Sync engine (delta push/pull, idempotency keys, retry queue) | **Implemented** (`Application/Sync`, `Infrastructure/Sync`): outbox captured by `SyncChangeCaptureInterceptor`, pull via `GET /api/v1/mobile/sync/{entity}`, writes with `Idempotency-Key`, exponential back-off, conflict = 409/412 | Contacts push+pull, Products pull only; other entities to register in `SyncCatalog` |
| Record sync metadata (`SyncStatus`, `ServerVersion`) | **Implemented** as `sync_entity_states` (no columns added to business tables) | `CreatedBy` not yet |
| Auth against web-api (token, refresh) | Client implemented (`SyncApiClient`, in-memory `SyncSession`, refresh on 401) | Sign-in dialog + Sync Now in the shell header (`SyncViewModel`, localized en/it/de/fr); token in memory only; sync stays off until `Sync:BaseUrl` is set |
| Executive DSS (Mod 093, issue #214): balanced scorecard, alerts, variance waterfall, profitability matrix + Whale Curve, what-if | **Implemented** (`Application/Dss`, `Domain/Dss`, `Infrastructure/Dss`, `Wpf/ExecutiveDssView`): snapshots pulled from `/api/v1/analytics/dss/*` into `dss_snapshots` per tenant, legal entity, currency, period and baseline; cockpit reads the cache first, so it works offline; `WhatIfScenarioCalculator` mirrors the server formulas | Board pack export needs a connection; custom period ranges are not cached; no radial gauge (progress bar), no drill-down to server documents |
| Project profitability and earned value (Mod 082, issue #190): CPI/SPI traffic lights, EAC/ETC (manual bottom-up ETC shown), margin, budget absorption, S-curve, profitability waterfall, WBS cost table, OIC 23 / Swiss GAAP FER 22 reconciliation | **Implemented** (`Application/ProjectEvm`, `Application/Features/ProjectEvm`, `Infrastructure/ProjectEvm`, `Wpf/ProjectEvmView`): snapshots pulled from `/api/v1/projects/{id}/evm/*` into `project_evm_snapshots` per tenant and project (the project picker has its own row); the view reads the cache first, so it works offline | Read-only: recomputing the snapshot, editing the manual ETC and the overhead allocation (Mod 018) stay on the web app; alerts (Mod 008) are shown as breached-threshold counts, not pushed |
| Industrial and analytical cost accounting (Mod 018, issue #168): cost and profit center hierarchy, contribution margin I/II and operating result, direct versus apportioned costs, step-down run history, manual analytical entry with allocation vectors | **Implemented** (`Application/AnalyticalAccounting`, `Application/Features/AnalyticalAccounting`, `Infrastructure/AnalyticalAccounting`, `Wpf/AnalyticalAccountingView`): read models pulled from `/api/v1/analytical/*` into `analytical_snapshots` per tenant; entries captured offline go to the `analytical_operations` queue and are sent first at the next sync; `AllocationVectorSplitter` mirrors the server Largest Remainder split | Centers, derivation rules, drivers and the step-down preview/consolidate/reverse cycle stay on the web app; refused queued entries are parked as failed without a review screen |
| Localization it/de/fr/en | Present (`Strings*.resx`) | Keys needed per module |
| CQRS/FluentValidation/Syncfusion shell | Present | — |

## Module coverage

| web-api module | Desktop | Status |
| :-- | :-- | :-- |
| 02_CRM (contacts) | ContactsView | Partial (no leads/campaigns/pipeline) |
| 05_Invoicing, 06_SDI | InvoicingView, SDI, QR-Bill | Partial (quotes + sales orders implemented in Application/Domain: workflow, conversion, invoice-from-order, migration; WPF view, delivery notes, credit notes still missing) |
| 13_Procurement | Purchase invoices | Partial (RFQ, PO, bids missing) |
| 14_Inventory | StockItem, Warehouse | Partial (transfers, movements, valuation) |
| 16_AssetManagement | FixedAsset + depreciation | Partial |
| 18_HumanResources / 18_Payroll | HR entities, CH/IT payroll | Partial (recruiting, shifts, compensation) |
| 22_GeneralLedger | Accounting view, VAT return | Partial (consolidation, period close) |
| 24_Treasury | Banking/finance | Partial |
| Time tracking | TimeTrackingView | Present |
| Privacy (nLPD/GDPR) | PrivacyView | Present |
| 15 Projects, 17 Service, 19 Subscription, 20 Analytics, 21 Workflow, 23 Comms, 25 Documents, 26 Contracts/FiscalRetail, 27–65 (≈45 vertical/enterprise modules) | — | **Missing** |

## Proposed delivery order
1. Sync foundation (API client, auth, outbox queue, audit columns, migration).
2. Invoicing completion: quotes, orders, delivery notes, credit notes.
3. CRM leads/pipeline, Procurement PO/RFQ, Inventory movements.
4. Remaining modules by customer demand.
