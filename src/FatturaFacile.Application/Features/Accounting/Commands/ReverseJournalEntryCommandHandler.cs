// <copyright file="ReverseJournalEntryCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Commands;

/// <summary>
/// Creates the storno of a posted entry and saves it; the original entry stays untouched.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="mapper">The mapper.</param>
/// <param name="time">The clock.</param>
public sealed class ReverseJournalEntryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, TimeProvider time) : IRequestHandler<ReverseJournalEntryCommand, GeneralLedgerEntryDto>
{
    /// <inheritdoc/>
    public Task<GeneralLedgerEntryDto> Handle(ReverseJournalEntryCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<GeneralLedgerEntryDto> HandleInternalAsync(ReverseJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var entries = unitOfWork.Repository<GeneralLedgerEntry>();
        var original = await entries.GetByIdAsync(request.EntryId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Journal entry not found.");

        // Loading the lines attaches them to the tracked entry.
        _ = await unitOfWork.Repository<GeneralLedgerLine>().FindAsync(l => l.GeneralLedgerEntryId == original.Id, cancellationToken).ConfigureAwait(false);

        var already = (await entries.FindAsync(e => e.ReversalOfEntryId == original.Id, cancellationToken).ConfigureAwait(false)).Count > 0;
        var year = request.ReversalDate.Year;
        var prefix = string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{JournalNumbering.Prefix}-{year}-");
        var numbers = await entries.FindAsync(e => e.EntryNumber.StartsWith(prefix), cancellationToken).ConfigureAwait(false);

        var reversal = JournalRules.CreateReversal(
            original,
            already,
            request.ReversalDate.Date,
            request.Reason,
            JournalNumbering.Next(year, numbers.Select(static e => e.EntryNumber)),
            time.GetUtcNow().UtcDateTime);

        await entries.AddAsync(reversal, cancellationToken).ConfigureAwait(false);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return mapper.Map<GeneralLedgerEntryDto>(reversal);
    }
}
