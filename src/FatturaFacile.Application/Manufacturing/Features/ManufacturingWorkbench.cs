// <copyright file="ManufacturingWorkbench.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Manufacturing;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Everything the workbench shows, read from the local store.
/// </summary>
/// <param name="WorkCenters">The work centres.</param>
/// <param name="Boms">The BOMs with their lines.</param>
/// <param name="RoutingOperations">The routing operations.</param>
/// <param name="Orders">The orders with their operations.</param>
/// <param name="Plans">The MRP planned orders.</param>
/// <param name="Logs">The shop-floor logs.</param>
/// <param name="Outbox">The outbox entries.</param>
/// <param name="Telemetry">The latest cockpit snapshot.</param>
public sealed record ManufacturingWorkbench(IReadOnlyList<WorkCenter> WorkCenters, IReadOnlyList<BomHeader> Boms, IReadOnlyList<RoutingOperation> RoutingOperations, IReadOnlyList<ProductionOrder> Orders, IReadOnlyList<MrpPlan> Plans, IReadOnlyList<ShopFloorLog> Logs, IReadOnlyList<ManufacturingOutboxEntry> Outbox, TelemetrySnapshot? Telemetry);
