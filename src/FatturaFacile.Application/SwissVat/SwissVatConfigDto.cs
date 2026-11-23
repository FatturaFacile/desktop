// <copyright file="SwissVatConfigDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Data transfer object for Swiss VAT tenant configuration.
/// </summary>
/// <param name="Id">Configuration identifier.</param>
/// <param name="UidMwst">Swiss UID-MWST number.</param>
/// <param name="AccountingMethod">Effective or Saldosteuersatz.</param>
/// <param name="ConsiderationBasis">Agreed or Received.</param>
/// <param name="ReportingFrequency">Quarterly, Semiannual, or Monthly.</param>
/// <param name="SssRatePrimary">Primary Saldosteuersatz rate percentage.</param>
/// <param name="SssRateSecondary">Secondary Saldosteuersatz rate percentage.</param>
/// <param name="AutoDetectBezugsteuer">Whether automatic Bezugsteuer detection is active.</param>
/// <param name="UseCommercial005Rounding">Whether 0.05 CHF rounding is active.</param>
public sealed record SwissVatConfigDto(
    Guid Id,
    string UidMwst,
    SwissVatMethod AccountingMethod,
    SwissVatConsiderationBasis ConsiderationBasis,
    SwissVatFrequency ReportingFrequency,
    decimal? SssRatePrimary,
    decimal? SssRateSecondary,
    bool AutoDetectBezugsteuer,
    bool UseCommercial005Rounding);
