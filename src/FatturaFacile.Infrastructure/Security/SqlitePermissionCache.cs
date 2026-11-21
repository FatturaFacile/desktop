// <copyright file="SqlitePermissionCache.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;
using FatturaFacile.Application.Common.Security.Iam;
using Microsoft.Data.Sqlite;

namespace FatturaFacile.Infrastructure.Security;

/// <summary>
/// SQLite store of permission snapshots, one row per tenant, user and company. The permissions and field policies are
/// encrypted (AES-256-GCM) at rest, and a revoked row is never returned, so a user cannot read or reuse the rights of
/// another tenant, user or company, nor rights the server withdrew.
/// </summary>
/// <param name="connectionString">The SQLite connection string.</param>
/// <param name="encryptor">The column encryptor.</param>
public sealed class SqlitePermissionCache(string connectionString, AesGcmFieldEncryptor encryptor) : IPermissionCache
{
    private const string NoCompany = "";

    private const string CreateTable =
        "CREATE TABLE IF NOT EXISTS iam_permission_snapshots ("
        + "tenant_id TEXT NOT NULL, user_id TEXT NOT NULL, company_id TEXT NOT NULL, payload TEXT NOT NULL, "
        + "fetched_at TEXT NOT NULL, revoked_at TEXT NULL, PRIMARY KEY (tenant_id, user_id, company_id))";

    private const string Upsert =
        "INSERT INTO iam_permission_snapshots (tenant_id, user_id, company_id, payload, fetched_at, revoked_at) "
        + "VALUES ($tenant, $user, $company, $payload, $fetched, NULL) "
        + "ON CONFLICT (tenant_id, user_id, company_id) "
        + "DO UPDATE SET payload = excluded.payload, fetched_at = excluded.fetched_at, revoked_at = NULL";

    private const string Select =
        "SELECT payload, fetched_at FROM iam_permission_snapshots "
        + "WHERE tenant_id = $tenant AND user_id = $user AND company_id = $company AND revoked_at IS NULL";

    private readonly Lock _initLock = new();
    private bool _initialized;

    /// <inheritdoc/>
    public Task SaveAsync(PermissionSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return SaveCoreAsync(snapshot, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PermissionSnapshot?> LoadAsync(Guid tenantId, Guid userId, Guid? companyId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = Select;
        _ = command.Parameters.AddWithValue("$tenant", tenantId.ToString());
        _ = command.Parameters.AddWithValue("$user", userId.ToString());
        _ = command.Parameters.AddWithValue("$company", CompanyKey(companyId));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var json = encryptor.Decrypt(reader.GetString(0));
        var payload = JsonSerializer.Deserialize<Payload>(json ?? string.Empty);
        return payload is null
            ? null
            : new PermissionSnapshot(
                tenantId,
                userId,
                companyId,
                payload.Permissions,
                payload.FieldPolicies,
                DateTimeOffset.Parse(reader.GetString(1), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
    }

    /// <inheritdoc/>
    public async Task RevokeUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE iam_permission_snapshots SET revoked_at = $now WHERE user_id = $user AND revoked_at IS NULL";
        _ = command.Parameters.AddWithValue("$user", userId.ToString());
        _ = command.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture));
        _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task PurgeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM iam_permission_snapshots";
        _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string CompanyKey(Guid? companyId) => companyId?.ToString() ?? NoCompany;

    private async Task SaveCoreAsync(PermissionSnapshot snapshot, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new Payload([.. snapshot.Permissions], [.. snapshot.FieldPolicies]));

        await using var connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = Upsert;
        _ = command.Parameters.AddWithValue("$tenant", snapshot.TenantId.ToString());
        _ = command.Parameters.AddWithValue("$user", snapshot.UserId.ToString());
        _ = command.Parameters.AddWithValue("$company", CompanyKey(snapshot.CompanyId));
        _ = command.Parameters.AddWithValue("$payload", encryptor.Encrypt(payload));
        _ = command.Parameters.AddWithValue("$fetched", snapshot.FetchedAt.ToString("O", CultureInfo.InvariantCulture));
        _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        lock (_initLock)
        {
            if (_initialized)
            {
                return connection;
            }

            using var create = connection.CreateCommand();
            create.CommandText = CreateTable;
            _ = create.ExecuteNonQuery();
            _initialized = true;
        }

        return connection;
    }

    private sealed record Payload(string[] Permissions, FieldPolicyEntry[] FieldPolicies);
}
