// <copyright file="Address.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.ValueObjects;

/// <summary>
/// Represents an immutable postal physical address supporting Italian, Swiss, and cross-border international formats.
/// Encapsulates geographic address components required for electronic invoicing, tax reporting (such as SDI FatturaPA
/// <c>DatiAnagrafici</c>/<c>Sede</c>), and payment standards (such as Swiss QR-bill address structures).
/// </summary>
public sealed class Address : ValueObject
{

    /// <summary>
    /// Initializes a new instance of the <see cref="Address"/> class with validated components.
    /// </summary>
    /// <param name="street">The street name, thoroughfare, and building or suite number (e.g., "Via Roma 12" or "Bahnhofstrasse 10").</param>
    /// <param name="city">The municipality, town, or locality name (e.g., "Milano" or "Lugano").</param>
    /// <param name="postalCode">The postal routing code (e.g., Italian 5-digit CAP, Swiss 4-digit NPA, or international ZIP code).</param>
    /// <param name="provinceOrCanton">The two-letter administrative jurisdiction abbreviation
    /// (e.g., Italian province code "MI" or Swiss canton "TI"); empty for countries without province divisions.</param>
    /// <param name="countryCode">The two-letter ISO 3166-1 alpha-2 country code (e.g., "IT", "CH"); defaults to "IT" when omitted or whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="street"/>, <paramref name="city"/>, or <paramref name="postalCode"/> is empty or contains only whitespace.</exception>
    public Address(
        string street,
        string city,
        string postalCode,
        string provinceOrCanton,
        string countryCode)
    {
        Street = string.IsNullOrWhiteSpace(value: street)
                     ? throw new ArgumentException(
                                                   message: "Street cannot be empty",
                                                   paramName: nameof(street))
                     : street.Trim();

        City = string.IsNullOrWhiteSpace(value: city)
                   ? throw new ArgumentException(
                                                 message: "City cannot be empty",
                                                 paramName: nameof(city))
                   : city.Trim();

        PostalCode = string.IsNullOrWhiteSpace(value: postalCode)
                         ? throw new ArgumentException(
                                                       message: "PostalCode cannot be empty",
                                                       paramName: nameof(postalCode))
                         : postalCode.Trim();

        ProvinceOrCanton = provinceOrCanton?.Trim().ToUpperInvariant() ?? string.Empty;
        CountryCode      = string.IsNullOrWhiteSpace(value: countryCode) ? "IT" : countryCode.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Address"/> class for entity framework or serialization.
    /// </summary>
    private Address()
    {
        Street           = string.Empty;
        City             = string.Empty;
        PostalCode       = string.Empty;
        ProvinceOrCanton = string.Empty;
        CountryCode      = "IT";
    }

    /// <summary>
    /// Gets the normalized street name, thoroughfare, and building or suite number.
    /// </summary>
    public string Street { get; }

    /// <summary>
    /// Gets the municipality, town, or city name.
    /// </summary>
    public string City { get; }

    /// <summary>
    /// Gets the postal routing code (Italian CAP, Swiss NPA, or international postal code).
    /// </summary>
    public string PostalCode { get; }

    /// <summary>
    /// Gets the uppercase two-letter administrative jurisdiction code, such as an Italian province (e.g., "MI", "RM") or Swiss canton (e.g., "TI", "ZH").
    /// </summary>
    public string ProvinceOrCanton { get; }

    /// <summary>
    /// Gets the uppercase ISO 3166-1 alpha-2 two-letter country code (e.g., "IT", "CH", "DE").
    /// </summary>
    public string CountryCode { get; }

    /// <summary>
    /// Formats the address as a human-readable, standard single-line postal representation.
    /// </summary>
    /// <returns>A formatted postal string containing street, postal code, city, province/canton, and country code.</returns>
    public override string ToString() => $"{Street}, {PostalCode} {City} ({ProvinceOrCanton}) - {CountryCode}";

    /// <summary>
    /// Returns the constituent components used for value object equality evaluation.
    /// </summary>
    /// <returns>An enumerable sequence containing <see cref="Street"/>, <see cref="City"/>, <see cref="PostalCode"/>, <see cref="ProvinceOrCanton"/>, and <see cref="CountryCode"/>.</returns>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
        yield return ProvinceOrCanton;
        yield return CountryCode;
    }

}
