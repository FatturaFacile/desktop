// <copyright file="GetProductsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common;
using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Products;

/// <summary>
/// Query request to retrieve all products and services available within the current tenant catalog.
/// </summary>
public record GetProductsQuery : 
    IRequest<IReadOnlyList<ProductDto>>, 
    IQuery<IReadOnlyList<ProductDto>>;
