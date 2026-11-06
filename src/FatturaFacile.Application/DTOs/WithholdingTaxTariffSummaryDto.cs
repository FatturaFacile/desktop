// <copyright file="WithholdingTaxTariffSummaryDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Imported withholding tax tariffs of a canton for one validity date.
/// </summary>
/// <param name="Canton">The canton.</param>
/// <param name="ValidFrom">The validity date.</param>
/// <param name="TariffCodes">The number of tariff codes.</param>
/// <param name="Rows">The number of rows.</param>
public sealed record WithholdingTaxTariffSummaryDto(string Canton, DateTime ValidFrom, int TariffCodes, int Rows);
