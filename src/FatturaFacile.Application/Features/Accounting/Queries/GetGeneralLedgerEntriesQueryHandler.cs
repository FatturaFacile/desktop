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
public sealed class GetGeneralLedgerEntriesQueryHandler : IRequestHandler<GetGeneralLedgerEntriesQuery, IReadOnlyList<GeneralLedgerEntryDto>>
{
    private readonly IRepository<GeneralLedgerEntry> _repository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGeneralLedgerEntriesQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">The General Ledger repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public GetGeneralLedgerEntriesQueryHandler(IRepository<GeneralLedgerEntry> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<GeneralLedgerEntryDto>> Handle(GetGeneralLedgerEntriesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<GeneralLedgerEntryDto> result = _repository.Query()
            .OrderByDescending(e => e.PostingDate)
            .ThenByDescending(e => e.EntryNumber)
            .ProjectTo<GeneralLedgerEntryDto>(_mapper.ConfigurationProvider)
            .ToList();

        return Task.FromResult(result);
    }
}
