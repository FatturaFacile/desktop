// <copyright file="DropshipAddress.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// The delivery address of a dropshipment.
/// </summary>
/// <param name="Name">The recipient name.</param>
/// <param name="Street">The street.</param>
/// <param name="HouseNumber">The house number.</param>
/// <param name="PostalCode">The postal code.</param>
/// <param name="City">The city.</param>
/// <param name="Region">The region or canton.</param>
/// <param name="CountryCode">The ISO 3166 alpha-2 country code.</param>
/// <param name="Phone">The phone.</param>
/// <param name="Email">The email.</param>
/// <param name="DeliveryNotes">The delivery notes.</param>
public sealed record DropshipAddress(
    string? Name,
    string Street,
    string? HouseNumber,
    string PostalCode,
    string City,
    string? Region,
    string CountryCode,
    string? Phone,
    string? Email,
    string? DeliveryNotes);
