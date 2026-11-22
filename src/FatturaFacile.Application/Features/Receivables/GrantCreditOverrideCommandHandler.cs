// <copyright file="GrantCreditOverrideCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="GrantCreditOverrideCommand"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class GrantCreditOverrideCommandHandler(IReceivablesService service) : IRequestHandler<GrantCreditOverrideCommand, ReceivablesActionResult>
{
    /// <inheritdoc/>
    public Task<ReceivablesActionResult> Handle(GrantCreditOverrideCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.GrantOverrideAsync(request.Request, cancellationToken);
    }
}
