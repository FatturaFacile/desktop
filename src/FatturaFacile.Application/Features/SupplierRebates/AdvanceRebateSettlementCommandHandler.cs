// <copyright file="AdvanceRebateSettlementCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Handles <see cref="AdvanceRebateSettlementCommand"/>.
/// </summary>
/// <param name="writer">The server actions.</param>
public sealed class AdvanceRebateSettlementCommandHandler(ISupplierRebatesWriteService writer) : IRequestHandler<AdvanceRebateSettlementCommand, SupplierRebatesResult>
{
    /// <inheritdoc/>
    public Task<SupplierRebatesResult> Handle(AdvanceRebateSettlementCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return writer.AdvanceSettlementAsync(request.AgreementId, request.ProposalId, request.Action, request.Details, cancellationToken);
    }
}
