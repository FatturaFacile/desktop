using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.ValueObjects;

public sealed class Address(string street, string city, string postalCode, string provinceOrCanton, string countryCode) : ValueObject
{
    public string Street { get; } = string.IsNullOrWhiteSpace(street) ? throw new ArgumentException("Street cannot be empty", nameof(street)) : street.Trim();
    public string City { get; } = string.IsNullOrWhiteSpace(city) ? throw new ArgumentException("City cannot be empty", nameof(city)) : city.Trim();
    public string PostalCode { get; } = string.IsNullOrWhiteSpace(postalCode) ? throw new ArgumentException("PostalCode cannot be empty", nameof(postalCode)) : postalCode.Trim();
    public string ProvinceOrCanton { get; } = provinceOrCanton?.Trim().ToUpperInvariant() ?? string.Empty;
    public string CountryCode { get; } = string.IsNullOrWhiteSpace(countryCode) ? "IT" : countryCode.Trim().ToUpperInvariant();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
        yield return ProvinceOrCanton;
        yield return CountryCode;
    }

    public override string ToString() => $"{Street}, {PostalCode} {City} ({ProvinceOrCanton}) - {CountryCode}";
}
