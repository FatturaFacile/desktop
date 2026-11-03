// <copyright file="GetProductsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Ports;
using MediatR;

namespace FatturaFacile.Application.Features.Products;

/// <summary>
/// Handles queries for retrieving catalog products and mapping them to data transfer objects.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetProductsQueryHandler"/> class.
/// </remarks>
/// <param name="productRepository">The product repository dependency.</param>
/// <param name="mapper">The AutoMapper instance for product DTO projection.</param>
public class GetProductsQueryHandler(IProductRepository productRepository, IMapper mapper) :
    IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>,
    IQueryHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IMapper _mapper = mapper;


    /// <summary>
    /// Handles retrieval of all tenant products via MediatR pipeline.
    /// </summary>
    /// <param name="query">The query request.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>A collection of <see cref="ProductDto"/> items.</returns>
    public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllProductsAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<ProductDto>>(products);
    }

    /// <summary>
    /// Handles retrieval of all tenant products via application dispatcher.
    /// </summary>
    /// <param name="query">The query request.</param>
    /// <param name="cancellationToken">Cancellation token to observe during processing.</param>
    /// <returns>A collection of <see cref="ProductDto"/> items.</returns>
    public Task<IReadOnlyList<ProductDto>> HandleAsync(GetProductsQuery query, CancellationToken cancellationToken = default) => Handle(query, cancellationToken);
}
