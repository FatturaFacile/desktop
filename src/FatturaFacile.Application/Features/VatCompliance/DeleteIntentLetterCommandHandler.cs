// <copyright file="DeleteIntentLetterCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="DeleteIntentLetterCommand"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="time">The clock.</param>
public sealed class DeleteIntentLetterCommandHandler(IUnitOfWork unitOfWork, TimeProvider time) : IRequestHandler<DeleteIntentLetterCommand, bool>
{
    /// <inheritdoc/>
    public Task<bool> Handle(DeleteIntentLetterCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.LetterId, cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(Guid letterId, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<IntentLetter>();
        var letter = await repository.GetByIdAsync(letterId, cancellationToken).ConfigureAwait(false);
        if (letter is null || letter.DeletedAt is not null)
        {
            return false;
        }

        var used = (await unitOfWork.Repository<IntentLetterUsage>().FindAsync(u => u.IntentLetterId == letterId, cancellationToken).ConfigureAwait(false)).Sum(static u => u.Amount);
        if (IntentLetterPlafond.Consumed(letter, used) > 0m)
        {
            throw new InvalidOperationException(VatComplianceErrors.LetterInUse);
        }

        if (letter.ServerId is null)
        {
            // Never sent: nothing to tell the server.
            repository.Delete(letter);
        }
        else
        {
            letter.SyncState = VatComplianceSyncState.PendingDelete;
            letter.DeletedAt = time.GetUtcNow().UtcDateTime;
            repository.Update(letter);
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}
