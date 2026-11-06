// <copyright file="WithholdingTaxTariffParserTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Payroll.Swiss;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the ESTV tariff file parser.
/// </summary>
public sealed class WithholdingTaxTariffParserTests
{
    /// <summary>
    /// Record 06 fields are read at their fixed positions with two implied decimals; deletions are skipped.
    /// </summary>
    [Fact]
    public void Parse_ReadsFixedLengthRecords()
    {
        var header = "00ZH" + new string(' ', 15) + "20251201";
        var row = "0601ZH" + "A0N".PadRight(10) + "20260101000500000000010000 0000000000000412   ";
        var deleted = "0603ZH" + "A0N".PadRight(10) + "20250101000500000000010000 0000000000000400   ";
        var content = string.Join("\r\n", header, row, deleted, "99");

        var rows = WithholdingTaxTariffParser.Parse(content);

        var tariff = Assert.Single(rows);
        Assert.Equal("ZH", tariff.Canton);
        Assert.Equal("A0N", tariff.TariffCode);
        Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), tariff.ValidFrom);
        Assert.Equal(5000m, tariff.IncomeFrom);
        Assert.Equal(100m, tariff.IncomeStep);
        Assert.Equal(0m, tariff.MinimumTax);
        Assert.Equal(4.12m, tariff.RatePercent);
        _ = Assert.Throws<FormatException>(() => WithholdingTaxTariffParser.Parse("00ZH"));
    }
}
