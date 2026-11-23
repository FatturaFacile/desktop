// <copyright file="GetErarioReconciliationQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="GetErarioReconciliationQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class GetErarioReconciliationQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetErarioReconciliationQuery, ErarioReconciliation>
{
    /// <inheritdoc/>
    public Task<ErarioReconciliation> Handle(GetErarioReconciliationQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Year, cancellationToken);
    }

    private async Task<ErarioReconciliation> HandleInternalAsync(int year, CancellationToken cancellationToken)
    {
        var returns = await unitOfWork.Repository<VatReturn>().FindAsync(r => r.Country == "IT" && r.PeriodFrom.Year == year, cancellationToken).ConfigureAwait(false);
        var settlements = await unitOfWork.Repository<VatSettlementSnapshot>().FindAsync(s => s.Year == year, cancellationToken).ConfigureAwait(false);
        var ledger = await ErarioLedgerReader.ReadAsync(unitOfWork, year, cancellationToken).ConfigureAwait(false);

        return ErarioReconciler.Reconcile(year, returns, settlements, ledger.AccountFound, ledger.Between);
    }
}
