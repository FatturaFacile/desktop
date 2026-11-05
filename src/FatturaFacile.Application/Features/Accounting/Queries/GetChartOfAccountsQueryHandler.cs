// <copyright file="GetChartOfAccountsQueryHandler.cs" company="Fattura Facile">
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
/// Handler for <see cref="GetChartOfAccountsQuery"/>.
/// </summary>
public sealed class GetChartOfAccountsQueryHandler : IRequestHandler<GetChartOfAccountsQuery, IReadOnlyList<ChartOfAccountDto>>
{
    private readonly IRepository<ChartOfAccount> _repository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetChartOfAccountsQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">The chart of accounts repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public GetChartOfAccountsQueryHandler(IRepository<ChartOfAccount> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ChartOfAccountDto>> Handle(GetChartOfAccountsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ChartOfAccountDto> result = _repository.Query()
            .Where(a => a.IsActive)
            .OrderBy(a => a.Code)
            .ProjectTo<ChartOfAccountDto>(_mapper.ConfigurationProvider)
            .ToList();

        return Task.FromResult(result);
    }
}
