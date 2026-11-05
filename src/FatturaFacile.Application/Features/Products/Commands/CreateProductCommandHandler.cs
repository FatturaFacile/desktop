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
public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateProductCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="currentTenantService">The current tenant service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public CreateProductCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenantService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentTenantService = currentTenantService;
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Product product = new()
        {
            TenantId = _currentTenantService.TenantId,
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

        await _unitOfWork.Repository<Product>().AddAsync(product, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return _mapper.Map<ProductDto>(product);
    }
}
