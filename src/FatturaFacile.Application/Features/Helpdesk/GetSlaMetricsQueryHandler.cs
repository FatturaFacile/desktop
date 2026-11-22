// <copyright file="GetSlaMetricsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Helpdesk;
using MediatR;

namespace FatturaFacile.Application.Features.Helpdesk;

/// <summary>
/// Handler for <see cref="GetSlaMetricsQuery"/>.
/// </summary>
/// <param name="service">The helpdesk service.</param>
public sealed class GetSlaMetricsQueryHandler(IHelpdeskService service) : IRequestHandler<GetSlaMetricsQuery, HelpdeskLoad<HelpdeskMetrics>?>
{
    /// <inheritdoc/>
    public Task<HelpdeskLoad<HelpdeskMetrics>?> Handle(GetSlaMetricsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadMetricsAsync(request.From, request.To, request.GroupBy, cancellationToken);
    }
}
