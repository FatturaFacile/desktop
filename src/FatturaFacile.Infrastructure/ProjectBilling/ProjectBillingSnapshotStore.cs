// <copyright file="ProjectBillingSnapshotStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FatturaFacile.Application.ProjectBilling;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.ProjectBilling;

/// <summary>
/// EF Core implementation of <see cref="IProjectBillingSnapshotStore"/> for the active tenant (the tenant filter of the context
/// keeps tenants apart). The snapshot lives in the project snapshot cache table that already serves the profitability view, in a row
/// keyed by <see cref="KeyOf"/> so it never meets the profitability row of the same project: the summary is in the first payload
/// column, the contract and the certificates in the second, the retention and the retainer in the third.
/// </summary>
/// <param name="db">The database context.</param>
/// <param name="tenant">The active tenant.</param>
public sealed class ProjectBillingSnapshotStore(FatturaFacileDbContext db, ICurrentTenantService tenant) : IProjectBillingSnapshotStore
{
    private const string KeyPrefix = "b";
    private const int MaxKeyLength = 36;
    private const string Contract = "contract";
    private const string Certificates = "certificates";
    private const string Retention = "retention";
    private const string Retainer = "retainer";
    private const string Absent = "null";

    /// <summary>
    /// Gets the key of the row of a project: a short prefix and the identifier without dashes, or a hash of it when it is longer.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <returns>The key, at most 36 characters.</returns>
    public static string KeyOf(string projectId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId);

        var key = KeyPrefix + projectId.Replace("-", string.Empty, StringComparison.Ordinal);

        return key.Length <= MaxKeyLength
            ? key
            : KeyPrefix + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(projectId)))[..(MaxKeyLength - KeyPrefix.Length)];
    }

    /// <inheritdoc/>
    public async Task<StoredProjectBillingPayload?> FindAsync(string projectId, CancellationToken cancellationToken)
    {
        var key = KeyOf(projectId);
        var row = await db.Set<LocalProjectEvmSnapshot>().Where(x => x.ProjectId == key).AsNoTracking().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (row is null)
        {
            return null;
        }

        using var second = JsonDocument.Parse(row.WaterfallJson);
        using var third = JsonDocument.Parse(row.ScurveJson);
        ProjectBillingPayload payload = new(
            row.BreakdownJson,
            Raw(second.RootElement, Contract),
            Raw(second.RootElement, Certificates, "[]"),
            Raw(third.RootElement, Retention),
            Raw(third.RootElement, Retainer));

        return new StoredProjectBillingPayload(payload, row.SyncedAtUtc);
    }

    /// <inheritdoc/>
    public Task SaveAsync(string projectId, ProjectBillingPayload payload, DateTime syncedAtUtc, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return WriteAsync(KeyOf(projectId), payload, syncedAtUtc, cancellationToken);
    }

    private static string Compose(params (string Name, string Json)[] parts) =>
        "{" + string.Join(',', parts.Select(p => $"\"{p.Name}\":{p.Json}")) + "}";

    private static string Raw(JsonElement parent, string name, string fallback = Absent) =>
        parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(name, out var value) ? value.GetRawText() : fallback;

    private async Task WriteAsync(string key, ProjectBillingPayload payload, DateTime syncedAtUtc, CancellationToken cancellationToken)
    {
        var row = await db.Set<LocalProjectEvmSnapshot>().Where(x => x.ProjectId == key).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (row is null)
        {
            row = new LocalProjectEvmSnapshot { TenantId = tenant.TenantId, ProjectId = key, CreatedAt = syncedAtUtc };
            _ = db.Set<LocalProjectEvmSnapshot>().Add(row);
        }
        else
        {
            row.UpdatedAt = syncedAtUtc;
        }

        row.BreakdownJson = payload.SummaryJson;
        row.WaterfallJson = Compose((Contract, payload.ContractJson), (Certificates, payload.CertificatesJson));
        row.ScurveJson = Compose((Retention, payload.RetentionJson), (Retainer, payload.RetainerJson));
        row.SyncedAtUtc = syncedAtUtc;

        // One SaveChanges: the views and their timestamp replace the previous snapshot together.
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
