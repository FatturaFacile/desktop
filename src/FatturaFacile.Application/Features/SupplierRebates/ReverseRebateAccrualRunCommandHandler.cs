// <copyright file="ReverseRebateAccrualRunCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Handles <see cref="ReverseRebateAccrualRunCommand"/>.
/// </summary>
/// <param name="writer">The server actions.</param>
public sealed class ReverseRebateAccrualRunCommandHandler(ISupplierRebatesWriteService writer) : IRequestHandler<ReverseRebateAccrualRunCommand, SupplierRebatesResult>
{
    /// <inheritdoc/>
    public Task<SupplierRebatesResult> Handle(ReverseRebateAccrualRunCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return writer.ReverseRunAsync(request.AgreementId, request.RunId, request.Reason, cancellationToken);
    }
}
