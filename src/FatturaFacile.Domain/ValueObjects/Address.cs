// <copyright file="Address.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.ValueObjects;

/// <summary>
/// Represents a postal physical address across Italian, Swiss, or international formats.
/// </summary>
public sealed class Address : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Address"/> class.
    /// </summary>
    /// <param name="street">The street address and building number.</param>
    /// <param name="city">The city or locality name.</param>
    /// <param name="postalCode">The postal code or CAP / NPA.</param>
    /// <param name="provinceOrCanton">The two-letter province or cantonal abbreviation.</param>
    /// <param name="countryCode">The ISO 3166-1 alpha-2 country code.</param>
    public Address(
        string street,
        string city,
        string postalCode,
        string provinceOrCanton,
        string countryCode)
    {
        Street = string.IsNullOrWhiteSpace(street)
            ? throw new ArgumentException(
                "Street cannot be empty",
                nameof(street))
            : street.Trim();
        City = string.IsNullOrWhiteSpace(city)
            ? throw new ArgumentException(
                "City cannot be empty",
                nameof(city))
            : city.Trim();
        PostalCode = string.IsNullOrWhiteSpace(postalCode)
            ? throw new ArgumentException(
                "PostalCode cannot be empty",
                nameof(postalCode))
            : postalCode.Trim();
        ProvinceOrCanton = provinceOrCanton?.Trim().ToUpperInvariant() ?? string.Empty;
        CountryCode = string.IsNullOrWhiteSpace(countryCode) ? "IT" : countryCode.Trim().ToUpperInvariant();
    }

    private Address()
    {
        Street = string.Empty;
        City = string.Empty;
        PostalCode = string.Empty;
        ProvinceOrCanton = string.Empty;
        CountryCode = "IT";
    }

    /// <summary>
    /// Gets the street address and building number.
    /// </summary>
    public string Street { get; }

    /// <summary>
    /// Gets the municipality or city name.
    /// </summary>
    public string City { get; }

    /// <summary>
    /// Gets the postal code (CAP in Italy, NPA in Switzerland).
    /// </summary>
    public string PostalCode { get; }

    /// <summary>
    /// Gets the province or cantonal jurisdiction code.
    /// </summary>
    public string ProvinceOrCanton { get; }

    /// <summary>
    /// Gets the ISO two-letter country code.
    /// </summary>
    public string CountryCode { get; }

    /// <inheritdoc/>
    public override string ToString() => $"{Street}, {PostalCode} {City} ({ProvinceOrCanton}) - {CountryCode}";

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
        yield return ProvinceOrCanton;
        yield return CountryCode;
    }
}
