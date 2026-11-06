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
/// <remarks>
/// Initializes a new instance of the <see cref="GetChartOfAccountsQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The chart of accounts repository.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class GetChartOfAccountsQueryHandler(IRepository<ChartOfAccount> repository, IMapper mapper) : IRequestHandler<GetChartOfAccountsQuery, IReadOnlyList<ChartOfAccountDto>>
{

    /// <inheritdoc/>
    public Task<IReadOnlyList<ChartOfAccountDto>> Handle(GetChartOfAccountsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ChartOfAccountDto> result = repository.Query()
            .Where(a => a.IsActive)
            .OrderBy(a => a.Code)
            .ProjectTo<ChartOfAccountDto>(mapper.ConfigurationProvider)
            .ToList();

        return Task.FromResult(result);
    }
}
