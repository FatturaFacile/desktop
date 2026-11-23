// <copyright file="SaveRebateAgreementCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Handles <see cref="SaveRebateAgreementCommand"/>.
/// </summary>
/// <param name="writer">The server actions.</param>
public sealed class SaveRebateAgreementCommandHandler(ISupplierRebatesWriteService writer) : IRequestHandler<SaveRebateAgreementCommand, SupplierRebatesResult>
{
    /// <inheritdoc/>
    public Task<SupplierRebatesResult> Handle(SaveRebateAgreementCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return writer.SaveAgreementAsync(request.AgreementId, request.Input, cancellationToken);
    }
}
