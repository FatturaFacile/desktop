// <copyright file="GetReceivableItemQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="GetReceivableItemQuery"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class GetReceivableItemQueryHandler(IReceivablesService service) : IRequestHandler<GetReceivableItemQuery, ReceivablesLoad<ReceivableItem>?>
{
    /// <inheritdoc/>
    public Task<ReceivablesLoad<ReceivableItem>?> Handle(GetReceivableItemQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadItemAsync(request.ItemId, cancellationToken);
    }
}
