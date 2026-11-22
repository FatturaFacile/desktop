// <copyright file="GetFieldServiceEquipmentQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="GetFieldServiceEquipmentQuery"/>.
/// </summary>
/// <param name="store">The local cache.</param>
public sealed class GetFieldServiceEquipmentQueryHandler(IFieldServiceStore store) : IRequestHandler<GetFieldServiceEquipmentQuery, FieldServiceCached<IReadOnlyList<FieldServiceEquipment>>?>
{
    /// <inheritdoc/>
    public Task<FieldServiceCached<IReadOnlyList<FieldServiceEquipment>>?> Handle(GetFieldServiceEquipmentQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return FieldServiceCacheReader.ReadAsync(store, FieldServiceCacheKinds.Equipment, FieldServiceCacheKinds.All, FieldServiceContractParser.ParseEquipment, cancellationToken);
    }
}
