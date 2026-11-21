// <copyright file="DismissMesConflictCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Mes;
using MediatR;

namespace FatturaFacile.Application.Features.Mes;

/// <summary>
/// Handler for <see cref="DismissMesConflictCommand"/>.
/// </summary>
/// <param name="service">The MES operator service.</param>
public sealed class DismissMesConflictCommandHandler(IMesOperatorService service) : IRequestHandler<DismissMesConflictCommand>
{
    /// <inheritdoc/>
    public Task Handle(DismissMesConflictCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.DismissConflictAsync(request.CommandId, cancellationToken);
    }
}
