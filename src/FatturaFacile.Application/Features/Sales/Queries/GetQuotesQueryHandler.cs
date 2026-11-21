// <copyright file="GetQuotesQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using AutoMapper.QueryableExtensions;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Sales.Queries;

/// <summary>
/// Handler for <see cref="GetQuotesQuery"/>.
/// </summary>
/// <param name="repository">The repository.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class GetQuotesQueryHandler(IRepository<Quote> repository, IMapper mapper) : IRequestHandler<GetQuotesQuery, IReadOnlyList<QuoteDto>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<QuoteDto>> Handle(GetQuotesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<QuoteDto> result = repository.Query()
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.QuoteNumber)
            .ProjectTo<QuoteDto>(mapper.ConfigurationProvider)
            .ToList();

        return Task.FromResult(result);
    }
}
