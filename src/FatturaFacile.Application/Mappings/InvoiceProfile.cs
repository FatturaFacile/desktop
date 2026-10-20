using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Sales;

namespace FatturaFacile.Application.Mappings;

public sealed class InvoiceProfile : Profile
{
    public InvoiceProfile()
    {
        _ = CreateMap<InvoiceLine, InvoiceLineDto>()
            .ForCtorParam(nameof(InvoiceLineDto.LineTotal), opt => opt.MapFrom(src => src.LineTotal));

        _ = CreateMap<ItalianElectronicInvoice, ItalianInvoiceDto>()
            .ForCtorParam(nameof(ItalianInvoiceDto.Currency), opt => opt.MapFrom(src => src.Currency.ToString()))
            .ForCtorParam(nameof(ItalianInvoiceDto.Lines), opt => opt.MapFrom(src => src.Lines));

        _ = CreateMap<SwissQrBill, SwissQrBillDto>()
            .ForCtorParam(nameof(SwissQrBillDto.CreditorAddressFormatted), opt => opt.MapFrom(src => src.CreditorAddress.ToString()))
            .ForCtorParam(nameof(SwissQrBillDto.CreditorQrIban), opt => opt.MapFrom(src => src.CreditorQrIban.Value))
            .ForCtorParam(nameof(SwissQrBillDto.DebtorAddressFormatted), opt => opt.MapFrom(src => src.DebtorAddress.ToString()))
            .ForCtorParam(nameof(SwissQrBillDto.Amount), opt => opt.MapFrom(src => src.Amount.Amount))
            .ForCtorParam(nameof(SwissQrBillDto.Currency), opt => opt.MapFrom(src => src.Amount.Currency.ToString()))
            .ForCtorParam(nameof(SwissQrBillDto.AdditionalMessage), opt => opt.MapFrom(src => src.AdditionalUnstructuredMessage));
    }
}
