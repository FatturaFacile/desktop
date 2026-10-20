using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Clients;

public record CreateClientCommand(
    string BusinessName,
    Jurisdiction Jurisdiction,
    string? FiscalCode,
    string? VatNumber,
    string? SdiCode,
    string? PecEmail,
    string? SwissUid,
    string? QrIban,
    string? CantonalCode,
    string Street,
    string City,
    string PostalCode,
    string ProvinceOrCanton,
    string CountryCode,
    string Email,
    string? PhoneNumber,
    int PaymentTermDays = 30) : IRequest<ClientDto>;