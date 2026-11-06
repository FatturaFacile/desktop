// <copyright file="FinalizeVatReturnCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Taxation;

/// <summary>
/// Handler for <see cref="FinalizeVatReturnCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FinalizeVatReturnCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="journal">The journal writer.</param>
public sealed class FinalizeVatReturnCommandHandler(IUnitOfWork unitOfWork, JournalWriter journal) : IRequestHandler<FinalizeVatReturnCommand, bool>
{
    /// <inheritdoc/>
    public Task<bool> Handle(FinalizeVatReturnCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Id, cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        var vatReturn = await unitOfWork.Repository<VatReturn>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (vatReturn is null || !string.Equals(vatReturn.Status, "Draft", StringComparison.Ordinal))
        {
            return false;
        }

        if (vatReturn.InputVat != 0m)
        {
            var outputAccount = await journal.AccountAsync(StandardAccount.OutputVat, cancellationToken).ConfigureAwait(false);
            var inputAccount = await journal.AccountAsync(StandardAccount.InputVat, cancellationToken).ConfigureAwait(false);
            _ = await journal.WriteAsync(
                "IVA",
                vatReturn.PeriodTo,
                string.Create(CultureInfo.InvariantCulture, $"Liquidazione IVA {vatReturn.PeriodFrom:dd.MM.yyyy} - {vatReturn.PeriodTo:dd.MM.yyyy}"),
                vatReturn.Id,
                [(outputAccount, vatReturn.InputVat, 0m), (inputAccount, 0m, vatReturn.InputVat)],
                cancellationToken).ConfigureAwait(false);
        }

        vatReturn.Status = "Final";
        unitOfWork.Repository<VatReturn>().Update(vatReturn);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
