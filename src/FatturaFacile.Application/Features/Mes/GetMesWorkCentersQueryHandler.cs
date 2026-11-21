// <copyright file="GetMesWorkCentersQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Mes;
using MediatR;

namespace FatturaFacile.Application.Features.Mes;

/// <summary>
/// Handler for <see cref="GetMesWorkCentersQuery"/>.
/// </summary>
/// <param name="service">The MES operator service.</param>
public sealed class GetMesWorkCentersQueryHandler(IMesOperatorService service) : IRequestHandler<GetMesWorkCentersQuery, IReadOnlyList<MesWorkCenter>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<MesWorkCenter>> Handle(GetMesWorkCentersQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.GetWorkCentersAsync(cancellationToken);
    }
}
