# Manufacturing workbench: offline store, outbox and conflict policy

The Engineering and Shop-Floor Supervisor workbench (navigation entry "Ingegneria e Reparto" / "Engineering & Shop Floor")
reads everything from a local SQLite store and never blocks on the network. Mutations are queued in an outbox and replayed
idempotently against `/api/v1/manufacturing` when the server is reachable.

## Store

`ManufacturingDbContext` lives in its own file (`fattura-facile-manufacturing.sqlite`, next to the ERP database) and has its
own migration (`Infrastructure/Manufacturing/Migrations`). It is a rebuildable cache plus the outbox, so the ERP schema is untouched.
Every table is tenant scoped through a global query filter.

| Table | Entity | Source |
| --- | --- | --- |
| `MfgWorkCenters`, `MfgItemCosts` | `WorkCenter`, `ItemCost` | `GET /work-centers`, `GET /items` (standard cost prices the BOM) |
| `MfgBomHeaders`, `MfgBomLines` | `BomHeader`, `BomLine` | `GET /boms`, `GET /boms/{id}` |
| `MfgRoutingOperations` | `RoutingOperation` | `GET /routings`, `GET /routings/{id}` |
| `MfgMrpPlans` | `MrpPlan` | `GET /mrp/planned-orders` |
| `MfgProductionOrders`, `MfgProductionOperations` | `ProductionOrder`, `ProductionOperation` | `GET /production-orders`, `GET /production-orders/{id}` |
| `MfgShopFloorLogs` | `ShopFloorLog` | local confirmations (key: `OperationPostingUuid`) |
| `MfgTelemetrySnapshots` | `TelemetrySnapshot` | `GET /cockpit` (last 20 kept) |
| `MfgOutbox` | `ManufacturingOutboxEntry` | local mutations |
| `MfgCheckpoints` | `ManufacturingCheckpoint` | delta checkpoint per resource |

## Delta pull

For every resource: `updated_since` = last checkpoint (the server may ignore it), page through the list, compare each row's
`updated_at` with the version stored locally and fetch the detail (components, operations) only for changed rows.
The checkpoint is the instant *before* the first request, so rows changed during the pull are fetched again next time.
Server-side hard deletes are not propagated (the list endpoints expose no tombstones).

## Outbox

* One row per mutation: `Kind`, route template (`{order}` is replaced by the server order id at send time), JSON payload,
  `IdempotencyKey`, status, attempts, `NextAttemptAt`, `Resolution`.
* The payload and the key are written when the mutation is queued and **never regenerated**. For an operation confirmation
  the key is the `operation_posting_uuid` that is also inside the payload and in the local `ShopFloorLog`.
* A retry after a lost response therefore sends the identical request; the server answers **HTTP 200** (replay of an already
  applied posting, the original confirmation is returned) instead of 201. 200 on a confirmation is counted as `Replayed`
  and the entry becomes `Synced`. Nothing is ever posted twice.
* The local effect (status change, operation quantities, order output) is applied in the same transaction as the queueing, so
  the workbench shows the result immediately while offline.
* Order is FIFO. Transient failures (network, 408, 429, 5xx) stop the run and back off with the shared `SyncRetryPolicy`
  (15 s doubling, 1 h cap, 8 attempts). 401 stops the run and asks for sign-in. Other 4xx mark the entry `Failed`.
* A `Conflict`, `Failed` or backed-off entry holds back the later entries of the **same order only**; other orders keep flowing.
* An order created offline keeps its local id; the creation response carries the server id (`ServerId`), which later entries
  use. Operation confirmations need the server operation ids, so they are only accepted for orders the server already knows.
* Orders with unresolved outbox entries are never overwritten by a pull (`HasPendingChanges`).

## Conflict policy on reconnect

HTTP 409, 412 and 422 are treated as a conflict. The resolver reads the current server status of the order
(`GET /production-orders/{id}`) and decides. Server state wins for the lifecycle; shop-floor postings are append-only events
and are never merged by content.

| Queued mutation | Server status | Decision |
| --- | --- | --- |
| any | cancelled | **ServerWins**: entry `Discarded`, optimistic effect taken back |
| Release / Start / Complete / Close | already at or past the target | **AlreadyApplied**: entry `Synced` |
| Release / Start / Complete / Close | exactly the required predecessor | **Reapply** once on the next flush |
| Release / Start / Complete / Close | anything else, or second conflict | **ManualReview**: entry `Conflict`, order blocked |
| Confirm operation | completed or closed | **ServerWins**: posting `Discarded`, quantities reverted |
| Confirm operation | released or in progress | **Reapply** once, then **ManualReview** |
| Confirm operation | planned | **ManualReview** |
| Create order | any conflict | **ManualReview** (possible duplicate) |
| any | server order unreadable | **ManualReview** (or retried later when the network dropped) |

If the server state cannot be read because the network dropped meanwhile, the entry is retried later instead of decided.

## Costing and scheduling (offline)

* WIP costing (`WipCostingCalculator`, decimal): standard labour = (labour min/unit x qty + setup) / 60 x labour rate,
  standard overhead = (run min/unit x qty + setup) / 60 x overhead rate, material from the multi-level BOM explosion at the
  items' standard cost. Actual = server actuals + postings still in the outbox. Variance = actual - standard allowed for the
  output so far; WIP = actual - standard value received. Amounts round to 2 decimals away from zero.
* Scheduling (`FiniteCapacityScheduler`): forward, orders by priority then earliest start; Monday to Friday from 08:00;
  daily capacity = hours x efficiency x availability of the work centre; operations of an order are sequential.
