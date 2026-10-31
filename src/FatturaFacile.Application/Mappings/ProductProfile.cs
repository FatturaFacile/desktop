// <copyright file="ProductProfile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Mappings;

/// <summary>
/// AutoMapper profile configuring projections between catalog product entities and product DTOs.
/// </summary>
public class ProductProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductProfile"/> class and registers mapping configurations.
    /// </summary>
    public ProductProfile()
    {
        _ = CreateMap<Product, ProductDto>();
    }
}
