// <copyright file="GetUnreconciledPaymentsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Receivables;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Handler for <see cref="GetUnreconciledPaymentsQuery"/>.
/// </summary>
/// <param name="service">The receivables service.</param>
public sealed class GetUnreconciledPaymentsQueryHandler(IReceivablesService service) : IRequestHandler<GetUnreconciledPaymentsQuery, ReceivablesLoad<IReadOnlyList<UnreconciledPayment>>?>
{
    /// <inheritdoc/>
    public Task<ReceivablesLoad<IReadOnlyList<UnreconciledPayment>>?> Handle(GetUnreconciledPaymentsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadUnreconciledPaymentsAsync(request.CustomerId, cancellationToken);
    }
}
