using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Entities.Purchasing;

namespace FatturaFacile.Application.Mappings;

public sealed class SupplierProfile : Profile
{
    public SupplierProfile()
    {
        _ = CreateMap<Supplier, SupplierDto>()
            .ForCtorParam(nameof(SupplierDto.VatNumber), opt => opt.MapFrom(src => src.VatNumber != null ? src.VatNumber.Value : null))
            .ForCtorParam(nameof(SupplierDto.AddressFormatted), opt => opt.MapFrom(src => src.Address != null ? src.Address.ToString() : string.Empty));

        _ = CreateMap<PurchaseOrder, PurchaseOrderDto>()
            .ForCtorParam(nameof(PurchaseOrderDto.SupplierName), opt => opt.MapFrom(src => $"Supplier #{src.SupplierId.ToString().Substring(0, 6)}"))
            .ForCtorParam(nameof(PurchaseOrderDto.TotalAmount), opt => opt.MapFrom(src => src.TotalAmount.Amount))
            .ForCtorParam(nameof(PurchaseOrderDto.Currency), opt => opt.MapFrom(src => src.TotalAmount.Currency.ToString()));
    }
}
