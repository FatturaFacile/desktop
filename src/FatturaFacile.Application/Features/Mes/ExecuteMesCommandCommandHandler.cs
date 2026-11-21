// <copyright file="ExecuteMesCommandCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Mes;
using MediatR;

namespace FatturaFacile.Application.Features.Mes;

/// <summary>
/// Handler for <see cref="ExecuteMesCommandCommand"/>.
/// </summary>
/// <param name="service">The MES operator service.</param>
public sealed class ExecuteMesCommandCommandHandler(IMesOperatorService service) : IRequestHandler<ExecuteMesCommandCommand, MesExecuteResult>
{
    /// <inheritdoc/>
    public Task<MesExecuteResult> Handle(ExecuteMesCommandCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.ExecuteAsync(request.Command, request.Row, cancellationToken);
    }
}
