// <copyright file="GetBadDebtProvisionQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="GetBadDebtProvisionQuery"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class GetBadDebtProvisionQueryHandler(IReceivablesService service) : IRequestHandler<GetBadDebtProvisionQuery, ReceivablesLoad<ProvisionSummary>?>
{
    /// <inheritdoc/>
    public Task<ReceivablesLoad<ProvisionSummary>?> Handle(GetBadDebtProvisionQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadProvisionAsync(request.CustomerId, request.Currency, cancellationToken);
    }
}
