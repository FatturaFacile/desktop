// <copyright file="DispatchTechnicianWorkOrderCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="DispatchTechnicianWorkOrderCommand"/>.
/// </summary>
/// <param name="api">The web-api client.</param>
public sealed class DispatchTechnicianWorkOrderCommandHandler(IFieldServiceApiClient api) : IRequestHandler<DispatchTechnicianWorkOrderCommand, FieldServiceDispatchResult>
{
    /// <inheritdoc/>
    public async Task<FieldServiceDispatchResult> Handle(DispatchTechnicianWorkOrderCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!api.IsAuthenticated)
        {
            return new FieldServiceDispatchResult(FieldServiceCallStatus.Unauthorized, [], Detail: null);
        }

        IReadOnlyList<FieldServiceWarning> warnings = [];
        var route = "work-orders/" + Uri.EscapeDataString(request.WorkOrderId) + "/dispatch";

        var result = await FieldServiceCallRunner.RunAsync(
            async () =>
            {
                var reply = await api.SendAsync(HttpMethod.Post, route, FieldServiceRequests.Dispatch(request.TechnicianId, request.Start, request.End), cancellationToken).ConfigureAwait(false);
                var status = FieldServiceCallRunner.Classify(reply.Status);
                if (status == FieldServiceCallStatus.Ok)
                {
                    return new FieldServiceCallResult(status, Id: request.WorkOrderId);
                }

                // The server answers 422 with all the blockers at once (schedule, skills, van stock).
                warnings = status == FieldServiceCallStatus.Rejected ? FieldServiceDispatchPreflight.FromServer(FieldServiceErrorReader.Blockers(reply.Body)) : [];

                return new FieldServiceCallResult(status, FieldServiceErrorReader.Describe(reply.Body));
            },
            cancellationToken).ConfigureAwait(false);

        return new FieldServiceDispatchResult(result.Status, warnings, result.Detail);
    }
}
