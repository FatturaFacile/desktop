// <copyright file="GetDispatcherGanttAndMapBoardQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="GetDispatcherGanttAndMapBoardQuery"/>.
/// </summary>
/// <param name="store">The local cache.</param>
public sealed class GetDispatcherGanttAndMapBoardQueryHandler(IFieldServiceStore store) : IRequestHandler<GetDispatcherGanttAndMapBoardQuery, FieldServiceCached<FieldServiceBoard>?>
{
    /// <inheritdoc/>
    public Task<FieldServiceCached<FieldServiceBoard>?> Handle(GetDispatcherGanttAndMapBoardQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return FieldServiceCacheReader.ReadAsync(store, FieldServiceCacheKinds.Board, FieldServiceCacheKinds.All, FieldServiceBoardParser.Parse, cancellationToken);
    }
}
