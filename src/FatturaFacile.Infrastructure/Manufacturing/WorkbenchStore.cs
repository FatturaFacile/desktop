// <copyright file="WorkbenchStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing;
using FatturaFacile.Application.Manufacturing.Outbox;
using FatturaFacile.Application.Manufacturing.Sync;
using FatturaFacile.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Manufacturing;

/// <summary>
/// EF Core SQLite implementation of the manufacturing read, outbox and delta-pull ports. Every call uses its own short-lived context, so the store is a safe singleton.
/// </summary>
/// <param name="factory">The context factory.</param>
/// <param name="tenant">The current tenant.</param>
/// <param name="timeProvider">The clock.</param>
public sealed partial class WorkbenchStore(IDbContextFactory<ManufacturingDbContext> factory, ICurrentTenantService tenant, TimeProvider timeProvider)
    : IManufacturingReadStore, IManufacturingOutboxStore, IManufacturingSnapshotStore
{
    private const int MaxLogs = 200;
    private const int MaxOutbox = 200;
    private const int MaxTelemetrySnapshots = 20;

    private DateTime Now => timeProvider.GetUtcNow().UtcDateTime;
}
