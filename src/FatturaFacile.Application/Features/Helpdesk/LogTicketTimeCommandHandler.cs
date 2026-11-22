// <copyright file="LogTicketTimeCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Helpdesk;
using MediatR;

namespace FatturaFacile.Application.Features.Helpdesk;

/// <summary>
/// Handler for <see cref="LogTicketTimeCommand"/>.
/// </summary>
/// <param name="service">The helpdesk service.</param>
public sealed class LogTicketTimeCommandHandler(IHelpdeskService service) : IRequestHandler<LogTicketTimeCommand, HelpdeskActionResult>
{
    /// <inheritdoc/>
    public Task<HelpdeskActionResult> Handle(LogTicketTimeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LogTimeAsync(request.Request, cancellationToken);
    }
}
