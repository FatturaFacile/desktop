// <copyright file="FlushMesOutboxCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Mes;
using MediatR;

namespace FatturaFacile.Application.Features.Mes;

/// <summary>
/// Handler for <see cref="FlushMesOutboxCommand"/>.
/// </summary>
/// <param name="service">The MES operator service.</param>
public sealed class FlushMesOutboxCommandHandler(IMesOperatorService service) : IRequestHandler<FlushMesOutboxCommand, MesFlushResult>
{
    /// <inheritdoc/>
    public Task<MesFlushResult> Handle(FlushMesOutboxCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.FlushAsync(cancellationToken);
    }
}
