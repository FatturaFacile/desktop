// <copyright file="GetAnnualVatReconciliationQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="GetAnnualVatReconciliationQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class GetAnnualVatReconciliationQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAnnualVatReconciliationQuery, AnnualVatReconciliation>
{
    /// <inheritdoc/>
    public Task<AnnualVatReconciliation> Handle(GetAnnualVatReconciliationQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Year, cancellationToken);
    }

    private async Task<AnnualVatReconciliation> HandleInternalAsync(int year, CancellationToken cancellationToken)
    {
        var returns = await unitOfWork.Repository<VatReturn>().FindAsync(r => r.Country == "IT" && r.PeriodFrom.Year == year, cancellationToken).ConfigureAwait(false);
        var register = await unitOfWork.Repository<VatRegisterEntry>()
            .FindAsync(e => e.Year == year && (e.Kind == VatRegisterKind.Sales || e.Kind == VatRegisterKind.Purchases), cancellationToken)
            .ConfigureAwait(false);
        var ledger = await ErarioLedgerReader.ReadAsync(unitOfWork, year, cancellationToken).ConfigureAwait(false);

        return AnnualVatReconciler.Build(year, returns, register, ledger.Total());
    }
}
