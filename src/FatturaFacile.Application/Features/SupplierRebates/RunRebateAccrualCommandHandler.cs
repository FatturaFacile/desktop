// <copyright file="RunRebateAccrualCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Handles <see cref="RunRebateAccrualCommand"/>.
/// </summary>
/// <param name="writer">The server actions.</param>
public sealed class RunRebateAccrualCommandHandler(ISupplierRebatesWriteService writer) : IRequestHandler<RunRebateAccrualCommand, SupplierRebatesResult>
{
    /// <inheritdoc/>
    public Task<SupplierRebatesResult> Handle(RunRebateAccrualCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return writer.RunAccrualAsync(request.AgreementId, request.PeriodEnd, request.PeriodStart, cancellationToken);
    }
}
