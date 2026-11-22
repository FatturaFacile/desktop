// <copyright file="SaveMaintenanceContractCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using System.Text.Json;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="SaveMaintenanceContractCommand"/>.
/// </summary>
/// <param name="api">The web-api client.</param>
/// <param name="time">The clock.</param>
public sealed class SaveMaintenanceContractCommandHandler(IFieldServiceApiClient api, TimeProvider time) : IRequestHandler<SaveMaintenanceContractCommand, FieldServiceCallResult>
{
    /// <inheritdoc/>
    public Task<FieldServiceCallResult> Handle(SaveMaintenanceContractCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return api.IsAuthenticated
            ? FieldServiceCallRunner.RunAsync(() => SaveAsync(request.Contract, cancellationToken), cancellationToken)
            : Task.FromResult(new FieldServiceCallResult(FieldServiceCallStatus.Unauthorized));
    }

    private async Task<FieldServiceCallResult> SaveAsync(FieldServiceContractInput contract, CancellationToken cancellationToken)
    {
        var isNew = contract.Id is null;
        var method = isNew ? HttpMethod.Post : HttpMethod.Patch;
        var route = isNew ? "maintenance-contracts" : "maintenance-contracts/" + Uri.EscapeDataString(contract.Id ?? string.Empty);

        var reply = await api.SendAsync(method, route, FieldServiceRequests.Contract(contract), cancellationToken).ConfigureAwait(false);
        var status = FieldServiceCallRunner.Classify(reply.Status);
        if (status != FieldServiceCallStatus.Ok)
        {
            return new FieldServiceCallResult(status, FieldServiceErrorReader.Describe(reply.Body));
        }

        using var document = JsonDocument.Parse(reply.Body);
        var data = FieldServiceJson.Child(document.RootElement, "data") ?? default;

        return new FieldServiceCallResult(status, AtUtc: time.GetUtcNow().UtcDateTime, Id: FieldServiceJson.NullableText(data, "id") ?? contract.Id);
    }
}
