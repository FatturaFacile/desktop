// <copyright file="CreateProductCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Products.Commands;

/// <summary>
/// Handler for <see cref="CreateProductCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateProductCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentTenantService">The current tenant service.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class CreateProductCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenantService,
    IMapper mapper) : IRequestHandler<CreateProductCommand, ProductDto>
{
    /// <inheritdoc/>
    public Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<ProductDto> HandleInternalAsync(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        Product product = new()
        {
            TenantId = currentTenantService.TenantId,
            Sku = request.Sku,
            Barcode = request.Barcode,
            Name = request.Name,
            Description = request.Description,
            UnitMeasure = request.UnitMeasure,
            CostPrice = request.CostPrice,
            SalePrice = request.SalePrice,
            TaxRate = request.TaxRate,
            HsCode = request.HsCode,
            TrackInventory = request.TrackInventory,
            IsActive = true,
        };

        await unitOfWork.Repository<Product>().AddAsync(product, cancellationToken)
           .ConfigureAwait(false);

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return mapper.Map<ProductDto>(product);
    }
}
