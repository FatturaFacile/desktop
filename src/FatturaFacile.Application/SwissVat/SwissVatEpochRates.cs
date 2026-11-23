// <copyright file="SwissVatEpochRates.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Statutory Swiss VAT rates and AFC box codes for a specific rate epoch.
/// </summary>
/// <param name="StandardRate">Standard rate percentage (8.1 or 7.7).</param>
/// <param name="ReducedRate">Reduced rate percentage (2.6 or 2.5).</param>
/// <param name="AccommodationRate">Special accommodation rate percentage (3.8 or 3.7).</param>
/// <param name="StandardBox">AFC box code for the standard rate (303 or 302).</param>
/// <param name="ReducedBox">AFC box code for the reduced rate (313 or 312).</param>
/// <param name="AccommodationBox">AFC box code for accommodation (343 or 342).</param>
/// <param name="BezugsteuerBox">AFC box code for Bezugsteuer (383 or 382).</param>
/// <param name="SssPrimaryBox">AFC box code for primary Saldosteuersatz (323 or 322).</param>
/// <param name="SssSecondaryBox">AFC box code for secondary Saldosteuersatz (333 or 332).</param>
public sealed record SwissVatEpochRates(
    decimal StandardRate,
    decimal ReducedRate,
    decimal AccommodationRate,
    string StandardBox,
    string ReducedBox,
    string AccommodationBox,
    string BezugsteuerBox,
    string SssPrimaryBox,
    string SssSecondaryBox);
