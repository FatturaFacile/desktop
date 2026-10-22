// <copyright file="SupplierProfile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Entities.Purchasing;

namespace FatturaFacile.Application.Mappings;

/// <summary>
/// AutoMapper profile configuring mappings for vendor profiles and purchase orders to data transfer objects.
/// </summary>
public sealed class SupplierProfile : Profile
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SupplierProfile"/> class and registers mapping strategies.
    /// </summary>
    public SupplierProfile()
    {
        _ = CreateMap<Supplier, SupplierDto>()
           .ForCtorParam(
                         ctorParamName: nameof(SupplierDto.VatNumber),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.VatNumber != null ? src.VatNumber.Value : null))
           .ForCtorParam(
                         ctorParamName: nameof(SupplierDto.AddressFormatted),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.Address != null ? src.Address.ToString() : string.Empty));

        _ = CreateMap<PurchaseOrder, PurchaseOrderDto>()
           .ForCtorParam(
                         ctorParamName: nameof(PurchaseOrderDto.SupplierName),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => $"Supplier #{src.SupplierId.ToString().Substring(0, 6)}"))
           .ForCtorParam(
                         ctorParamName: nameof(PurchaseOrderDto.TotalAmount),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.TotalAmount.Amount))
           .ForCtorParam(
                         ctorParamName: nameof(PurchaseOrderDto.Currency),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.TotalAmount.Currency.ToString()));
    }

}
