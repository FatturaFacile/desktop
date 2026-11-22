// <copyright file="GetReceivableItemsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="GetReceivableItemsQuery"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class GetReceivableItemsQueryHandler(IReceivablesService service) : IRequestHandler<GetReceivableItemsQuery, ReceivablesLoad<ReceivableItemPage>?>
{
    /// <inheritdoc/>
    public Task<ReceivablesLoad<ReceivableItemPage>?> Handle(GetReceivableItemsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadItemsAsync(request.Filter, cancellationToken);
    }
}
