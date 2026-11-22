// <copyright file="FlushHelpdeskOutboxCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Helpdesk;
using MediatR;

namespace FatturaFacile.Application.Features.Helpdesk;

/// <summary>
/// Handler for <see cref="FlushHelpdeskOutboxCommand"/>.
/// </summary>
/// <param name="service">The helpdesk service.</param>
public sealed class FlushHelpdeskOutboxCommandHandler(IHelpdeskService service) : IRequestHandler<FlushHelpdeskOutboxCommand, HelpdeskFlushResult>
{
    /// <inheritdoc/>
    public Task<HelpdeskFlushResult> Handle(FlushHelpdeskOutboxCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.FlushAsync(cancellationToken);
    }
}
