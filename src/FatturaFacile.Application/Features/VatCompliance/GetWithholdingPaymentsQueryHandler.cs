// <copyright file="GetWithholdingPaymentsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="GetWithholdingPaymentsQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class GetWithholdingPaymentsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetWithholdingPaymentsQuery, IReadOnlyList<WithholdingF24Payment>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<WithholdingF24Payment>> Handle(GetWithholdingPaymentsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Year, cancellationToken);
    }

    private async Task<IReadOnlyList<WithholdingF24Payment>> HandleInternalAsync(int year, CancellationToken cancellationToken) =>
        [.. (await unitOfWork.Repository<WithholdingF24Payment>().FindAsync(p => p.Year == year, cancellationToken).ConfigureAwait(false)).OrderBy(static p => p.Month).ThenBy(static p => p.TaxCode, StringComparer.Ordinal)];
}
