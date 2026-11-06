// <copyright file="GetProductsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Products.Queries;

/// <summary>
/// Query to retrieve products and services in the current tenant catalog.
/// </summary>
public sealed record GetProductsQuery : IRequest<IReadOnlyList<ProductDto>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
