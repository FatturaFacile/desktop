// <copyright file="SaveIntentLetterCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="SaveIntentLetterCommand"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class SaveIntentLetterCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<SaveIntentLetterCommand, Guid>
{
    /// <inheritdoc/>
    public Task<Guid> Handle(SaveIntentLetterCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Input, cancellationToken);
    }

    private async Task<Guid> HandleInternalAsync(IntentLetterInput input, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<IntentLetter>();
        var protocol = input.ProtocolNumber.Trim();
        var duplicates = await repository.FindAsync(l => l.ProtocolNumber == protocol && l.DeletedAt == null, cancellationToken).ConfigureAwait(false);
        if (duplicates.Any(l => l.Id != input.Id))
        {
            throw new InvalidOperationException(VatComplianceErrors.ProtocolDuplicate);
        }

        IntentLetter letter;
        if (input.Id is { } id)
        {
            letter = await repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false) ?? throw new KeyNotFoundException(VatComplianceErrors.Required);

            // A plafond cannot go below what invoices already used.
            var used = (await unitOfWork.Repository<IntentLetterUsage>().FindAsync(u => u.IntentLetterId == id, cancellationToken).ConfigureAwait(false)).Sum(static u => u.Amount);
            if (input.PlafondAmount < IntentLetterPlafond.Consumed(letter, used))
            {
                throw new InvalidOperationException(VatComplianceErrors.LetterInUse);
            }
        }
        else
        {
            letter = new IntentLetter();
            await repository.AddAsync(letter, cancellationToken).ConfigureAwait(false);
        }

        letter.ContactId = input.ContactId;
        letter.Direction = input.Direction;
        letter.ProtocolNumber = protocol;
        letter.ReceiptDate = input.ReceiptDate.Date;
        letter.Year = input.Year;
        letter.PlafondAmount = input.PlafondAmount;
        letter.ValidFrom = input.ValidFrom.Date;
        letter.ValidTo = input.ValidTo.Date;
        letter.Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim();
        letter.SyncError = null;
        letter.SyncState = letter.ServerId is null ? VatComplianceSyncState.PendingCreate : VatComplianceSyncState.PendingUpdate;

        if (input.Id is not null)
        {
            repository.Update(letter);
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return letter.Id;
    }
}
