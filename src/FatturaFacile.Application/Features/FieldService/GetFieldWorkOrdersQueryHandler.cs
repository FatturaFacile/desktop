// <copyright file="GetFieldWorkOrdersQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="GetFieldWorkOrdersQuery"/>.
/// </summary>
/// <param name="store">The local cache.</param>
public sealed class GetFieldWorkOrdersQueryHandler(IFieldServiceStore store) : IRequestHandler<GetFieldWorkOrdersQuery, FieldServiceCached<IReadOnlyList<FieldServiceWorkOrder>>?>
{
    /// <inheritdoc/>
    public Task<FieldServiceCached<IReadOnlyList<FieldServiceWorkOrder>>?> Handle(GetFieldWorkOrdersQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return FieldServiceCacheReader.ReadAsync(store, FieldServiceCacheKinds.WorkOrders, FieldServiceCacheKinds.All, FieldServiceWorkOrderParser.ParseList, cancellationToken);
    }
}
