// <copyright file="GetMesBoardQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Mes;
using MediatR;

namespace FatturaFacile.Application.Features.Mes;

/// <summary>
/// Handler for <see cref="GetMesBoardQuery"/>.
/// </summary>
/// <param name="service">The MES operator service.</param>
public sealed class GetMesBoardQueryHandler(IMesOperatorService service) : IRequestHandler<GetMesBoardQuery, MesBoardView?>
{
    /// <inheritdoc/>
    public Task<MesBoardView?> Handle(GetMesBoardQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadBoardAsync(request.WorkCenterId, cancellationToken);
    }
}
