// <copyright file="WithholdingTaxTariffParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Payroll.Swiss;

/// <summary>
/// Parses the official ESTV withholding tax tariff files (fixed-length records, "Recordformate Quellensteuer-Tarife"):
/// record 06 = tariff row (canton 5-6, code 7-16, valid from 17-24, income from 25-33, step 34-42, minimum tax 46-54,
/// rate 55-59; amounts and rate with two implied decimals).
/// </summary>
public static class WithholdingTaxTariffParser
{
    /// <summary>
    /// Parses a tariff file.
    /// </summary>
    /// <param name="content">The file content.</param>
    /// <returns>The tariff rows (deletion records are skipped).</returns>
    /// <exception cref="FormatException">When the file contains no tariff records.</exception>
    public static IReadOnlyList<WithholdingTaxTariff> Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        List<WithholdingTaxTariff> rows = [];

        foreach (var rawLine in content.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');

            if (line.Length < 59 || !line.StartsWith("06", StringComparison.Ordinal) || string.Equals(line[2..4], "03", StringComparison.Ordinal))
            {
                continue;
            }

            rows.Add(new WithholdingTaxTariff
            {
                Canton = line[4..6].Trim().ToUpperInvariant(),
                TariffCode = line[6..16].Trim().ToUpperInvariant(),
                ValidFrom = DateTime.ParseExact(line[16..24], "yyyyMMdd", CultureInfo.InvariantCulture),
                IncomeFrom = Implied(line[24..33]),
                IncomeStep = Implied(line[33..42]),
                MinimumTax = Implied(line[45..54]),
                RatePercent = Implied(line[54..59]),
            });
        }

        return rows.Count == 0
            ? throw new FormatException("Il file non contiene record di tariffa ESTV (record 06).")
            : rows;
    }

    private static decimal Implied(string digits)
    {
        var trimmed = digits.Trim();
        return trimmed.Length == 0 ? 0m : decimal.Parse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture) / 100m;
    }
}
