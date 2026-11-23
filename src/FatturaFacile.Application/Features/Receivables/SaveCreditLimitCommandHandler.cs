// <copyright file="SaveCreditLimitCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="SaveCreditLimitCommand"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class SaveCreditLimitCommandHandler(IReceivablesService service) : IRequestHandler<SaveCreditLimitCommand, ReceivablesActionResult>
{
    /// <inheritdoc/>
    public Task<ReceivablesActionResult> Handle(SaveCreditLimitCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.SaveCreditLimitAsync(request.Request, cancellationToken);
    }
}
