// <copyright file="GetCustomerEquipmentServiceHistoryQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="GetCustomerEquipmentServiceHistoryQuery"/>.
/// </summary>
/// <param name="store">The local cache.</param>
public sealed class GetCustomerEquipmentServiceHistoryQueryHandler(IFieldServiceStore store) : IRequestHandler<GetCustomerEquipmentServiceHistoryQuery, FieldServiceCached<EquipmentHistory>?>
{
    /// <inheritdoc/>
    public Task<FieldServiceCached<EquipmentHistory>?> Handle(GetCustomerEquipmentServiceHistoryQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return FieldServiceCacheReader.ReadAsync(store, FieldServiceCacheKinds.History, request.EquipmentId, FieldServiceContractParser.ParseHistory, cancellationToken);
    }
}
