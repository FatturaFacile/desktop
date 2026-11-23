// <copyright file="SwissVatCalculationInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Input payload for <see cref="SwissVatCalculator"/>.
/// </summary>
/// <param name="PeriodFrom">Period start date.</param>
/// <param name="PeriodTo">Period end date.</param>
/// <param name="Method">Reporting method (Effective or Saldosteuersatz).</param>
/// <param name="ConsiderationBasis">Agreed or Received consideration.</param>
/// <param name="SalesLines">Sales lines in the period.</param>
/// <param name="PurchaseLines">Purchase lines in the period.</param>
/// <param name="BezugsteuerEntries">Confirmed Bezugsteuer entries in the period.</param>
/// <param name="AdjustmentsBox410">Total approved Degrassazione (Box 410).</param>
/// <param name="AdjustmentsBox415">Total approved Autoconsumo / mixed-use reductions (Box 415).</param>
/// <param name="AdjustmentsBox420">Total approved subsidy reductions (Box 420).</param>
/// <param name="SssRatePrimary">Primary Saldosteuersatz rate percentage.</param>
/// <param name="SssRateSecondary">Secondary Saldosteuersatz rate percentage.</param>
/// <param name="UseCommercial005Rounding">True for 0.05 CHF rounding.</param>
public sealed record SwissVatCalculationInput(
    DateOnly PeriodFrom,
    DateOnly PeriodTo,
    SwissVatMethod Method,
    SwissVatConsiderationBasis ConsiderationBasis,
    IReadOnlyList<SwissVatSaleLineInput> SalesLines,
    IReadOnlyList<SwissVatPurchaseLineInput> PurchaseLines,
    IReadOnlyList<BezugsteuerEntryDto> BezugsteuerEntries,
    decimal AdjustmentsBox410 = 0m,
    decimal AdjustmentsBox415 = 0m,
    decimal AdjustmentsBox420 = 0m,
    decimal? SssRatePrimary = null,
    decimal? SssRateSecondary = null,
    bool UseCommercial005Rounding = true);
