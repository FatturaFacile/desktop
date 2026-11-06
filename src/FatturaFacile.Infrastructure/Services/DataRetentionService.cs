// <copyright file="DataRetentionService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Services;

/// <summary>
/// Deletes the working time records whose retention period (art. 73 cpv. 2 OLL 1) has expired. The deletion is done
/// in the database directly, because the application layer only allows logical deletion of these records; it is
/// itself recorded in the change history.
/// </summary>
/// <param name="context">The database context.</param>
public sealed class DataRetentionService(FatturaFacileDbContext context) : IDataRetentionService
{
    /// <inheritdoc />
    public Task<DataRetentionResult> ApplyAsync(Guid tenantId, DateTime today, CancellationToken cancellationToken = default) =>
        ApplyInternalAsync(tenantId, today, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<DataRetentionResult>> ApplyAllTenantsAsync(DateTime today, CancellationToken cancellationToken = default)
    {
        var tenantIds = await context.Tenants
            .IgnoreQueryFilters()
            .Where(t => t.DeletedAt == null)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<DataRetentionResult> results = [];

        foreach (var tenantId in tenantIds)
        {
            results.Add(await ApplyInternalAsync(tenantId, today, cancellationToken).ConfigureAwait(false));
        }

        return results;
    }

    private async Task<DataRetentionResult> ApplyInternalAsync(Guid tenantId, DateTime today, CancellationToken cancellationToken)
    {
        var tenant = await context.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Azienda non trovata.");
        var cutoff = DataRetentionPolicy.Cutoff(today, tenant.TimeRecordRetentionYears);

        var clockEvents = await context.TimeClockEvents
            .IgnoreQueryFilters()
            .Where(e => e.TenantId == tenantId && e.Timestamp < cutoff)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
        var dailyRecords = await context.TimeDailyRecords
            .IgnoreQueryFilters()
            .Where(r => r.TenantId == tenantId && r.Date < cutoff)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
        var auditEntries = await context.TimeAuditEntries
            .IgnoreQueryFilters()
            .Where(a => a.TenantId == tenantId && a.Timestamp < cutoff)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);

        DataRetentionResult result = new(tenantId, cutoff, clockEvents, dailyRecords, auditEntries);

        if (result.Total > 0)
        {
            _ = context.TimeAuditEntries.Add(new TimeAuditEntry
            {
                TenantId = tenantId,
                EmployeeId = Guid.Empty,
                EntityType = "Retention",
                EntityId = Guid.Empty,
                Action = "RetentionPurge",
                After = string.Create(
                    CultureInfo.InvariantCulture,
                    $"Eliminati i dati anteriori al {cutoff:dd.MM.yyyy}: {clockEvents} timbrature, {dailyRecords} registrazioni giornaliere, {auditEntries} voci di storico."),
                Reason = "Termine di conservazione scaduto (art. 73 cpv. 2 OLL 1)",
                UserName = "Sistema",
                Timestamp = DateTime.Now,
            });
        }

        tenant.LastRetentionRunAt = DateTime.UtcNow;
        _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }
}
