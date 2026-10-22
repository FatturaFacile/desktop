// <copyright file="CreateClientCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;

using MediatR;

namespace FatturaFacile.Application.Features.Clients;

/// <summary>
/// Command to register and onboard a new client entity under either Italian or Swiss fiscal regulations.
/// </summary>
/// <param name="BusinessName">The legal registered company denomination or client full name.</param>
/// <param name="Jurisdiction">The operational legal jurisdiction (<see cref="Jurisdiction.Italy"/> or <see cref="Jurisdiction.Switzerland"/>).</param>
/// <param name="FiscalCode">The Italian Codice Fiscale, required for Italian clients.</param>
/// <param name="VatNumber">The Italian Partita IVA number, required for Italian commercial clients.</param>
/// <param name="SdiCode">The Italian SDI 7-character electronic invoice routing recipient code.</param>
/// <param name="PecEmail">The Italian certified electronic email address (Posta Elettronica Certificata).</param>
/// <param name="SwissUid">The Swiss enterprise identifier (UID/IDE), required for Swiss business clients.</param>
/// <param name="QrIban">The optional Swiss QR-IBAN account number designated for receivables.</param>
/// <param name="CantonalCode">The two-letter Swiss canton abbreviation (e.g., "TI", "ZH").</param>
/// <param name="Street">The street name and street number of the client billing address.</param>
/// <param name="City">The municipality or town name of the client address.</param>
/// <param name="PostalCode">The postal code (CAP / NPA).</param>
/// <param name="ProvinceOrCanton">The province code (Italy) or cantonal abbreviation (Switzerland).</param>
/// <param name="CountryCode">The two-letter ISO country code (e.g., "IT", "CH").</param>
/// <param name="Email">The primary electronic mail contact address.</param>
/// <param name="PhoneNumber">The telephone contact number.</param>
/// <param name="PaymentTermDays">The agreed commercial credit period in days from billing (defaults to 30).</param>
public record CreateClientCommand(
    string       BusinessName,
    Jurisdiction Jurisdiction,
    string?      FiscalCode,
    string?      VatNumber,
    string?      SdiCode,
    string?      PecEmail,
    string?      SwissUid,
    string?      QrIban,
    string?      CantonalCode,
    string       Street,
    string       City,
    string       PostalCode,
    string       ProvinceOrCanton,
    string       CountryCode,
    string       Email,
    string?      PhoneNumber,
    int          PaymentTermDays = 30) : IRequest<ClientDto>;
