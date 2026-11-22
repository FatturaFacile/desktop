// <copyright file="CreateJournalEntryCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Commands;

/// <summary>
/// Creates a Prima Nota entry: numbers it, enforces the double-entry rules when it is posted and stores the entry with its lines in one save.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentTenantService">The active tenant.</param>
/// <param name="mapper">The mapper.</param>
/// <param name="time">The clock.</param>
public sealed class CreateJournalEntryCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenantService,
    IMapper mapper,
    TimeProvider time) : IRequestHandler<CreateJournalEntryCommand, GeneralLedgerEntryDto>
{
    /// <inheritdoc/>
    public Task<GeneralLedgerEntryDto> Handle(CreateJournalEntryCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<GeneralLedgerEntryDto> HandleInternalAsync(CreateJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var accountIds = request.Lines.Select(static l => l.AccountId).Distinct().ToList();
        var known = await unitOfWork.Repository<ChartOfAccount>().FindAsync(a => accountIds.Contains(a.Id), cancellationToken).ConfigureAwait(false);
        if (known.Count != accountIds.Count)
        {
            throw new JournalValidationException(JournalViolation.MissingAccount);
        }

        var entries = unitOfWork.Repository<GeneralLedgerEntry>();
        var year = request.PostingDate.Year;
        var prefix = string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{JournalNumbering.Prefix}-{year}-");
        var existing = await entries.FindAsync(e => e.EntryNumber.StartsWith(prefix), cancellationToken).ConfigureAwait(false);

        GeneralLedgerEntry entry = new()
        {
            TenantId = currentTenantService.TenantId,
            EntryNumber = JournalNumbering.Next(year, existing.Select(static e => e.EntryNumber)),
            PostingDate = request.PostingDate.Date,
            DocumentDate = request.PostingDate.Date,
            Description = request.Description.Trim(),
            SourceDocumentType = request.SourceDocumentType,
            LedgerType = request.LedgerType,
        };

        var number = 1;
        foreach (var line in request.Lines.Where(static l => l.Debit != 0m || l.Credit != 0m))
        {
            entry.Lines.Add(new GeneralLedgerLine
            {
                GeneralLedgerEntryId = entry.Id,
                AccountId = line.AccountId,
                LineNumber = number++,
                Debit = line.Debit,
                Credit = line.Credit,
                Description = string.IsNullOrWhiteSpace(line.Description) ? entry.Description : line.Description,
                CostCenterId = line.CostCenterId,
            });
        }

        if (request.PostImmediately)
        {
            JournalRules.Post(entry, time.GetUtcNow().UtcDateTime);
        }

        await entries.AddAsync(entry, cancellationToken).ConfigureAwait(false);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return mapper.Map<GeneralLedgerEntryDto>(entry);
    }
}
