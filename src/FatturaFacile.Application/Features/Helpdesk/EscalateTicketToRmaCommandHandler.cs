// <copyright file="EscalateTicketToRmaCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Helpdesk;
using MediatR;

namespace FatturaFacile.Application.Features.Helpdesk;

/// <summary>
/// Handler for <see cref="EscalateTicketToRmaCommand"/>.
/// </summary>
/// <param name="service">The helpdesk service.</param>
public sealed class EscalateTicketToRmaCommandHandler(IHelpdeskService service) : IRequestHandler<EscalateTicketToRmaCommand, HelpdeskActionResult>
{
    /// <inheritdoc/>
    public Task<HelpdeskActionResult> Handle(EscalateTicketToRmaCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.EscalateToRmaAsync(request.Request, cancellationToken);
    }
}
