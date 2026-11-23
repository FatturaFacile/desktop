// <copyright file="DeleteRebateContributionCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;
using MediatR;

namespace FatturaFacile.Application.Features.SupplierRebates;

/// <summary>
/// Handles <see cref="DeleteRebateContributionCommand"/>.
/// </summary>
/// <param name="writer">The server actions.</param>
public sealed class DeleteRebateContributionCommandHandler(ISupplierRebatesWriteService writer) : IRequestHandler<DeleteRebateContributionCommand, SupplierRebatesResult>
{
    /// <inheritdoc/>
    public Task<SupplierRebatesResult> Handle(DeleteRebateContributionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return writer.DeleteContributionAsync(request.AgreementId, request.ContributionId, cancellationToken);
    }
}
