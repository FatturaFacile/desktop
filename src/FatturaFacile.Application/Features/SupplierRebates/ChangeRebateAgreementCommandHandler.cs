// <copyright file="ChangeRebateAgreementCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Handles <see cref="ChangeRebateAgreementCommand"/>.
/// </summary>
/// <param name="writer">The server actions.</param>
public sealed class ChangeRebateAgreementCommandHandler(ISupplierRebatesWriteService writer) : IRequestHandler<ChangeRebateAgreementCommand, SupplierRebatesResult>
{
    /// <inheritdoc/>
    public Task<SupplierRebatesResult> Handle(ChangeRebateAgreementCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return writer.ChangeAgreementAsync(request.AgreementId, request.Action, cancellationToken);
    }
}
