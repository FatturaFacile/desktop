// <copyright file="GetGeneralLedgerEntriesQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using AutoMapper.QueryableExtensions;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Queries;

/// <summary>
/// Handler for <see cref="GetGeneralLedgerEntriesQuery"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetGeneralLedgerEntriesQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The General Ledger repository.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class GetGeneralLedgerEntriesQueryHandler(IRepository<GeneralLedgerEntry> repository, IMapper mapper) : IRequestHandler<GetGeneralLedgerEntriesQuery, IReadOnlyList<GeneralLedgerEntryDto>>
{

    /// <inheritdoc/>
    public Task<IReadOnlyList<GeneralLedgerEntryDto>> Handle(GetGeneralLedgerEntriesQuery request, CancellationToken cancellationToken)
    {
        var result = repository.Query()
            .OrderByDescending(e => e.PostingDate)
            .ThenByDescending(e => e.EntryNumber)
            .ProjectTo<GeneralLedgerEntryDto>(mapper.ConfigurationProvider)
            .ToList();

        var reversed = repository.Query()
            .Where(e => e.ReversalOfEntryId != null)
            .Select(e => e.ReversalOfEntryId!.Value)
            .ToHashSet();

        foreach (var entry in result)
        {
            entry.IsReversed = reversed.Contains(entry.Id);
        }

        return Task.FromResult<IReadOnlyList<GeneralLedgerEntryDto>>(result);
    }
}
