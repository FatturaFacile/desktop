// <copyright file="LipeRules.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;

namespace FatturaFacile.Application.Taxation;

/// <summary>
/// The ministerial content rules of a LIPE communication that the structural schema cannot express: tax identifiers, the period
/// and the consistency of the figures.
/// </summary>
public static partial class LipeRules
{
    /// <summary>
    /// The first year a LIPE communication exists for.
    /// </summary>
    public const int FirstYear = 2017;

    private const decimal Tolerance = 0.01m;
    private const int MonthsInYear = 12;
    private const int QuartersInYear = 4;
    private const int PersonTaxCodeLength = 16;

    [GeneratedRegex("^[A-Z]{6}[0-9LMNPQRSTUV]{2}[A-Z][0-9LMNPQRSTUV]{2}[A-Z][0-9LMNPQRSTUV]{3}[A-Z]$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PersonTaxCodePattern { get; }

    /// <summary>
    /// Checks the data of a communication.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <returns>The codes of the violated rules; empty when the data is consistent.</returns>
    public static IReadOnlyList<string> Check(LipeData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        List<string> errors = [];

        if (!IsTaxCode(data.TaxCode))
        {
            errors.Add("tax_code_invalid");
        }

        if (!PartitaIva.IsValid(data.VatNumber))
        {
            errors.Add("vat_number_invalid");
        }

        if (!string.IsNullOrWhiteSpace(data.DeclarantTaxCode) && !IsTaxCode(data.DeclarantTaxCode))
        {
            errors.Add("declarant_tax_code_invalid");
        }

        if (data.Year < FirstYear)
        {
            errors.Add("year_invalid");
        }

        var hasMonth = data.Month is >= 1 and <= MonthsInYear;
        var hasQuarter = data.Quarter is >= 1 and <= QuartersInYear;
        if (hasMonth == hasQuarter)
        {
            errors.Add("period_invalid");
        }

        if (new[] { data.ActiveOperations, data.PassiveOperations, data.OutputVat, data.InputVat, data.PreviousCredit, data.Interest }.Any(static v => v < 0m))
        {
            errors.Add("negative_amount");
        }

        var expected = data.OutputVat - data.InputVat - data.PreviousCredit + data.Interest;
        if (data.Payable > Tolerance && Math.Abs(data.Payable - expected) > Tolerance)
        {
            errors.Add("payable_inconsistent");
        }

        if (data.Payable < -Tolerance && Math.Abs(-data.Payable - Math.Max(0m, -expected)) > Tolerance)
        {
            errors.Add("credit_inconsistent");
        }

        return errors;
    }

    private static bool IsTaxCode(string? value) =>
        value is not null && (value.Length == PartitaIva.Length ? value.All(char.IsAsciiDigit) : value.Length == PersonTaxCodeLength && PersonTaxCodePattern.IsMatch(value));
}
