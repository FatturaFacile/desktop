// <copyright file="CreateClientCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Clients;

/// <summary>
/// Represents create client command.
/// </summary>
/// <param name="BusinessName">The business name.</param>
/// <param name="Jurisdiction">The jurisdiction.</param>
/// <param name="FiscalCode">The fiscal code.</param>
/// <param name="VatNumber">The vat number.</param>
/// <param name="SdiCode">The sdi code.</param>
/// <param name="PecEmail">The pec email.</param>
/// <param name="SwissUid">The swiss uid.</param>
/// <param name="QrIban">The qr iban.</param>
/// <param name="CantonalCode">The cantonal code.</param>
/// <param name="Street">The street.</param>
/// <param name="City">The city.</param>
/// <param name="PostalCode">The postal code.</param>
/// <param name="ProvinceOrCanton">The province or canton.</param>
/// <param name="CountryCode">The country code.</param>
/// <param name="Email">The email.</param>
/// <param name="PhoneNumber">The phone number.</param>
/// <param name="PaymentTermDays">The payment term days.</param>
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
