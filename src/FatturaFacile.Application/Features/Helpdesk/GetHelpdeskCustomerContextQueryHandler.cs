// <copyright file="GetHelpdeskCustomerContextQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Helpdesk;
using MediatR;

namespace FatturaFacile.Application.Features.Helpdesk;

/// <summary>
/// Handler for <see cref="GetHelpdeskCustomerContextQuery"/>.
/// </summary>
/// <param name="service">The helpdesk service.</param>
public sealed class GetHelpdeskCustomerContextQueryHandler(IHelpdeskService service) : IRequestHandler<GetHelpdeskCustomerContextQuery, HelpdeskLoad<HelpdeskCustomerContext>?>
{
    /// <inheritdoc/>
    public Task<HelpdeskLoad<HelpdeskCustomerContext>?> Handle(GetHelpdeskCustomerContextQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadCustomerContextAsync(request.TicketId, cancellationToken);
    }
}
