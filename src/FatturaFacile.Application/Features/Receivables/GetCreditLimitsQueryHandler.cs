// <copyright file="GetCreditLimitsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="GetCreditLimitsQuery"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class GetCreditLimitsQueryHandler(IReceivablesService service) : IRequestHandler<GetCreditLimitsQuery, ReceivablesLoad<IReadOnlyList<CustomerCreditLimit>>?>
{
    /// <inheritdoc/>
    public Task<ReceivablesLoad<IReadOnlyList<CustomerCreditLimit>>?> Handle(GetCreditLimitsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadCreditLimitsAsync(request.CustomerId, cancellationToken);
    }
}
