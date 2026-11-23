// <copyright file="DeleteCreditLimitCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="DeleteCreditLimitCommand"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class DeleteCreditLimitCommandHandler(IReceivablesService service) : IRequestHandler<DeleteCreditLimitCommand, ReceivablesActionResult>
{
    /// <inheritdoc/>
    public Task<ReceivablesActionResult> Handle(DeleteCreditLimitCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.DeleteCreditLimitAsync(request.Request, cancellationToken);
    }
}
