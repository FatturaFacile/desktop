// <copyright file="GetWithholdingTaxTariffSummaryQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Queries;

/// <summary>
/// Handler for <see cref="GetWithholdingTaxTariffSummaryQuery"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetWithholdingTaxTariffSummaryQueryHandler"/> class.
/// </remarks>
/// <param name="store">The tariff store.</param>
public sealed class GetWithholdingTaxTariffSummaryQueryHandler(IWithholdingTaxTariffStore store)
    : IRequestHandler<GetWithholdingTaxTariffSummaryQuery, IReadOnlyList<WithholdingTaxTariffSummaryDto>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<WithholdingTaxTariffSummaryDto>> Handle(GetWithholdingTaxTariffSummaryQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return store.GetSummaryAsync(cancellationToken);
    }
}
