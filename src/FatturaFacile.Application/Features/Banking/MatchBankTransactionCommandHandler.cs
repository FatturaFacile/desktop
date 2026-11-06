// <copyright file="MatchBankTransactionCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Banking;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Banking;

/// <summary>
/// Handler for <see cref="MatchBankTransactionCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MatchBankTransactionCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="reconciler">The reconciler.</param>
public sealed class MatchBankTransactionCommandHandler(IUnitOfWork unitOfWork, BankReconciler reconciler) : IRequestHandler<MatchBankTransactionCommand, bool>
{
    /// <inheritdoc/>
    public Task<bool> Handle(MatchBankTransactionCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(MatchBankTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await unitOfWork.Repository<BankTransaction>().GetByIdAsync(request.TransactionId, cancellationToken).ConfigureAwait(false);

        if (transaction is null)
        {
            return false;
        }

        var applied = await reconciler.ApplyAsync(transaction, request.DocumentType, request.DocumentId, cancellationToken).ConfigureAwait(false);

        if (applied)
        {
            unitOfWork.Repository<BankTransaction>().Update(transaction);
            _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return applied;
    }
}
