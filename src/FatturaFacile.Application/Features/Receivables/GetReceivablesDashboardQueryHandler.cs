// <copyright file="GetReceivablesDashboardQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="GetReceivablesDashboardQuery"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class GetReceivablesDashboardQueryHandler(IReceivablesService service) : IRequestHandler<GetReceivablesDashboardQuery, ReceivablesLoad<ReceivablesDashboard>?>
{
    /// <inheritdoc/>
    public Task<ReceivablesLoad<ReceivablesDashboard>?> Handle(GetReceivablesDashboardQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadDashboardAsync(request.Currency, cancellationToken);
    }
}
