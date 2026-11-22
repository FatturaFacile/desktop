// <copyright file="GetHelpdeskWorkbenchQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Helpdesk;
using MediatR;

namespace FatturaFacile.Application.Features.Helpdesk;

/// <summary>
/// Handler for <see cref="GetHelpdeskWorkbenchQuery"/>.
/// </summary>
/// <param name="service">The helpdesk service.</param>
public sealed class GetHelpdeskWorkbenchQueryHandler(IHelpdeskService service) : IRequestHandler<GetHelpdeskWorkbenchQuery, HelpdeskWorkbench>
{
    /// <inheritdoc/>
    public Task<HelpdeskWorkbench> Handle(GetHelpdeskWorkbenchQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadWorkbenchAsync(request.RiskPercent, cancellationToken);
    }
}
