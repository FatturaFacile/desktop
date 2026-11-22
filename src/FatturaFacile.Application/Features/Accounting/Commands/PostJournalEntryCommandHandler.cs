// <copyright file="PostJournalEntryCommandHandler.cs" company="Fattura Facile">
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
/// Posts a draft entry after checking the double-entry rules.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="mapper">The mapper.</param>
/// <param name="time">The clock.</param>
public sealed class PostJournalEntryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, TimeProvider time) : IRequestHandler<PostJournalEntryCommand, GeneralLedgerEntryDto>
{
    /// <inheritdoc/>
    public Task<GeneralLedgerEntryDto> Handle(PostJournalEntryCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<GeneralLedgerEntryDto> HandleInternalAsync(PostJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await unitOfWork.Repository<GeneralLedgerEntry>().GetByIdAsync(request.EntryId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Journal entry not found.");

        // Loading the lines attaches them to the tracked entry.
        _ = await unitOfWork.Repository<GeneralLedgerLine>().FindAsync(l => l.GeneralLedgerEntryId == entry.Id, cancellationToken).ConfigureAwait(false);

        JournalRules.Post(entry, time.GetUtcNow().UtcDateTime);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return mapper.Map<GeneralLedgerEntryDto>(entry);
    }
}
