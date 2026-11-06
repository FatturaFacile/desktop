// <copyright file="SwissQrBillAddress.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Strategies.Swiss;

/// <summary>
/// Structured address (type "S") as required on the Swiss QR-bill since the Swiss Implementation Guidelines v2.3.
/// </summary>
/// <param name="Name">Name or company (max 70).</param>
/// <param name="Street">Street name (max 70, optional).</param>
/// <param name="HouseNumber">Building number (max 16, optional).</param>
/// <param name="PostalCode">Postal code without country prefix (max 16).</param>
/// <param name="Town">Town (max 35).</param>
/// <param name="Country">ISO 3166-1 alpha-2 country code.</param>
public sealed record SwissQrBillAddress(
    string Name,
    string? Street,
    string? HouseNumber,
    string PostalCode,
    string Town,
    string Country);
