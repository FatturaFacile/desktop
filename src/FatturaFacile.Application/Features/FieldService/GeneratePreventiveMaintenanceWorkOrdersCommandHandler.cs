// <copyright file="GeneratePreventiveMaintenanceWorkOrdersCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Application.FieldService;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="GeneratePreventiveMaintenanceWorkOrdersCommand"/>.
/// </summary>
/// <param name="api">The web-api client.</param>
/// <param name="time">The clock.</param>
public sealed class GeneratePreventiveMaintenanceWorkOrdersCommandHandler(IFieldServiceApiClient api, TimeProvider time) : IRequestHandler<GeneratePreventiveMaintenanceWorkOrdersCommand, FieldServicePreventiveResult>
{
    private const string Route = "preventive-maintenance/run";

    /// <inheritdoc/>
    public Task<FieldServicePreventiveResult> Handle(GeneratePreventiveMaintenanceWorkOrdersCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return api.IsAuthenticated
            ? RunAsync(cancellationToken)
            : Task.FromResult(new FieldServicePreventiveResult(FieldServiceCallStatus.Unauthorized, 0, 0, 0, Detail: null));
    }

    private static int CountItems(IEnumerable<JsonElement> items)
    {
        var count = 0;
        foreach (var item in items)
        {
            count += item.ValueKind == JsonValueKind.Undefined ? 0 : 1;
        }

        return count;
    }

    private async Task<FieldServicePreventiveResult> RunAsync(CancellationToken cancellationToken)
    {
        var evaluated = 0;
        var created = 0;
        var skipped = 0;

        var result = await FieldServiceCallRunner.RunAsync(
            async () =>
            {
                var reply = await api.SendAsync(HttpMethod.Post, Route, jsonBody: null, cancellationToken).ConfigureAwait(false);
                var status = FieldServiceCallRunner.Classify(reply.Status);
                if (status != FieldServiceCallStatus.Ok)
                {
                    return new FieldServiceCallResult(status, FieldServiceErrorReader.Describe(reply.Body));
                }

                using var document = JsonDocument.Parse(reply.Body);
                var data = FieldServiceJson.Child(document.RootElement, "data") ?? default;
                evaluated = FieldServiceJson.Whole(data, "evaluated_assets");
                created = CountItems(FieldServiceJson.Items(data, "created"));
                skipped = FieldServiceJson.Whole(data, "skipped_existing");

                return new FieldServiceCallResult(FieldServiceCallStatus.Ok, AtUtc: time.GetUtcNow().UtcDateTime);
            },
            cancellationToken).ConfigureAwait(false);

        return new FieldServicePreventiveResult(result.Status, evaluated, created, skipped, result.Detail);
    }
}
