// <copyright file="FieldServiceBatch.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Builds the body of `POST /api/v1/fsm/mobile-sync` from the outbox entries. Every operation carries its `client_uuid`, which
/// is what makes a replay after a lost answer harmless: the server reports it as `duplicate` and changes nothing.
/// </summary>
public static class FieldServiceBatch
{
    /// <summary>
    /// The largest number of operations the server accepts in one batch.
    /// </summary>
    public const int MaxOperations = 200;

    /// <summary>
    /// Builds the batch body.
    /// </summary>
    /// <param name="entries">The entries, in the order the technician acted.</param>
    /// <returns>The JSON body.</returns>
    public static string Build(IEnumerable<FieldServiceOutboxEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var operations = entries
            .Select(entry => new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["client_uuid"] = entry.ClientUuid,
                ["type"] = TypeName(entry.Operation),
                ["work_order_id"] = entry.WorkOrderId.ToString("D", CultureInfo.InvariantCulture),
                ["payload"] = JsonSerializer.Deserialize<JsonElement>(entry.PayloadJson),
            })
            .ToList();

        return JsonSerializer.Serialize(new Dictionary<string, object?>(StringComparer.Ordinal) { ["operations"] = operations });
    }

    /// <summary>
    /// Gets the `type` of an operation as the server names it.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <returns>The wire name.</returns>
    public static string TypeName(FieldServiceOperationType operation) => operation switch
    {
        FieldServiceOperationType.StatusTransition => "status_transition",
        FieldServiceOperationType.PartConsumption => "part_consumption",
        FieldServiceOperationType.ServiceReport => "service_report",
        _ => "service_report",
    };
}
