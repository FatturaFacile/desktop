// <copyright file="GetSalesOrdersQueryHandler.cs" company="Fattura Facile">
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
/// Handler for <see cref="GetSalesOrdersQuery"/>.
/// </summary>
/// <param name="repository">The repository.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class GetSalesOrdersQueryHandler(IRepository<SalesOrder> repository, IMapper mapper) : IRequestHandler<GetSalesOrdersQuery, IReadOnlyList<SalesOrderDto>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<SalesOrderDto>> Handle(GetSalesOrdersQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<SalesOrderDto> result = repository.Query()
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.OrderNumber)
            .ProjectTo<SalesOrderDto>(mapper.ConfigurationProvider)
            .ToList();

        return Task.FromResult(result);
    }
}
