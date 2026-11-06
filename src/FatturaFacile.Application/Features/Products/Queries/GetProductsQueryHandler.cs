// <copyright file="GetProductsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using AutoMapper.QueryableExtensions;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Products.Queries;

/// <summary>
/// Handler for <see cref="GetProductsQuery"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetProductsQueryHandler"/> class.
/// </remarks>
/// <param name="repository">The product repository.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class GetProductsQueryHandler(IRepository<Product> repository, IMapper mapper) : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{

    /// <inheritdoc/>
    public Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ProductDto> result = repository.Query()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ProjectTo<ProductDto>(mapper.ConfigurationProvider)
            .ToList();

        return Task.FromResult(result);
    }
}
