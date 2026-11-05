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
public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IRepository<Product> _repository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetProductsQueryHandler"/> class.
    /// </summary>
    /// <param name="repository">The product repository.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public GetProductsQueryHandler(IRepository<Product> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ProductDto> result = _repository.Query()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
            .ToList();

        return Task.FromResult(result);
    }
}
