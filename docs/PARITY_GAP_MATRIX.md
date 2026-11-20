# web-api ↔ Desktop Parity Gap Matrix

Baseline: desktop `dotnet build -c Release` = 0 warnings / 0 errors.
web-api: 65 modules, ~600 controllers, ~3,150 OpenAPI paths (`docs/api/openapi.json`).

## Cross-cutting infrastructure

| Capability | Desktop state | Gap |
| :-- | :-- | :-- |
| Sync engine (delta push/pull, idempotency keys, retry queue) | **Implemented** (`Application/Sync`, `Infrastructure/Sync`): outbox captured by `SyncChangeCaptureInterceptor`, pull via `GET /api/v1/mobile/sync/{entity}`, writes with `Idempotency-Key`, exponential back-off, conflict = 409/412 | Contacts push+pull, Products pull only; other entities to register in `SyncCatalog` |
| Record sync metadata (`SyncStatus`, `ServerVersion`) | **Implemented** as `sync_entity_states` (no columns added to business tables) | `CreatedBy` not yet |
| Auth against web-api (token, refresh) | Client implemented (`SyncApiClient`, in-memory `SyncSession`, refresh on 401) | Sign-in dialog + Sync Now in the shell header (`SyncViewModel`, localized en/it/de/fr); token in memory only; sync stays off until `Sync:BaseUrl` is set |
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
