// <copyright file="SaveRebateContributionCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Handles <see cref="SaveRebateContributionCommand"/>.
/// </summary>
/// <param name="writer">The server actions.</param>
public sealed class SaveRebateContributionCommandHandler(ISupplierRebatesWriteService writer) : IRequestHandler<SaveRebateContributionCommand, SupplierRebatesResult>
{
    /// <inheritdoc/>
    public Task<SupplierRebatesResult> Handle(SaveRebateContributionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return writer.SaveContributionAsync(request.AgreementId, request.ContributionId, request.Input, cancellationToken);
    }
}
