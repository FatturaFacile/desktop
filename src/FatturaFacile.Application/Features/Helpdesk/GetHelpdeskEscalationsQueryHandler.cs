// <copyright file="GetHelpdeskEscalationsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Helpdesk;
using MediatR;

namespace FatturaFacile.Application.Features.Helpdesk;

/// <summary>
/// Handler for <see cref="GetHelpdeskEscalationsQuery"/>.
/// </summary>
/// <param name="service">The helpdesk service.</param>
public sealed class GetHelpdeskEscalationsQueryHandler(IHelpdeskService service) : IRequestHandler<GetHelpdeskEscalationsQuery, HelpdeskLoad<IReadOnlyList<HelpdeskEscalation>>?>
{
    /// <inheritdoc/>
    public Task<HelpdeskLoad<IReadOnlyList<HelpdeskEscalation>>?> Handle(GetHelpdeskEscalationsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadEscalationsAsync(request.TicketId, cancellationToken);
    }
}
