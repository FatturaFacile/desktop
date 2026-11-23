// <copyright file="VatRegisterReportTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the printable report of a VAT register: totals per rate, gaps, the CSV content, its seal and the printable table.
/// </summary>
public sealed class VatRegisterReportTests
{
    private static readonly DateTime From = new(2027, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
    private static readonly DateTime To = new(2027, 3, 31, 0, 0, 0, DateTimeKind.Unspecified);

    /// <summary>
    /// The report orders the lines by protocol and totals them per rate, with the deductible split of purchases.
    /// </summary>
    [Fact]
    public void Build_OrdersTheLinesAndTotalsPerRate()
    {
        var report = VatRegisterReportBuilder.Build(
            VatRegisterKind.Purchases,
            section: null,
            From,
            To,
            [
                Line(2, 22m, 200m, 44m, deductible: 33m, nonDeductible: 11m),
                Line(1, 22m, 100m, 22m, deductible: 22m, nonDeductible: 0m),
                Line(3, 0m, 50m, 0m, nature: "N4"),
            ]);

        int[] protocols = [1, 2, 3];

        Assert.Equal(protocols, report.Lines.Select(static l => l.ProtocolNumber).ToArray());
        Assert.Equal(350m, report.TotalTaxable);
        Assert.Equal(66m, report.TotalVat);
        Assert.Equal(55m, report.TotalDeductible);
        Assert.Equal(11m, report.TotalNonDeductible);
        var ordinary = Assert.Single(report.RateTotals, static t => t.VatRate == 22m);
        Assert.Equal(300m, ordinary.Taxable);
        var exempt = Assert.Single(report.RateTotals, static t => t.VatRate == 0m);
        Assert.Equal("N4", exempt.Nature);
    }

    /// <summary>
    /// A hole in the numbering and the locked lines are reported.
    /// </summary>
    [Fact]
    public void Build_ReportsGapsAndFinalizedLines()
    {
        var locked = Line(1, 22m, 100m, 22m);
        locked.IsFinalized = true;

        var report = VatRegisterReportBuilder.Build(VatRegisterKind.Sales, VatRegisterSection.Ordinary, From, To, [locked, Line(4, 22m, 100m, 22m)]);

        int[] gaps = [2, 3];

        Assert.Equal(gaps, report.Gaps.ToArray());
        Assert.Equal(1, report.FinalizedLines);
    }

    /// <summary>
    /// The CSV has invariant numbers, quoted text with escaped quotes and a byte order mark, and its seal changes with the content.
    /// </summary>
    [Fact]
    public void Csv_UsesInvariantNumbersAndSealsTheContent()
    {
        var line = Line(1, 22m, 1234.5m, 271.59m);
        line.PartnerName = "Rossi \"Srl\"";
        var report = VatRegisterReportBuilder.Build(VatRegisterKind.Sales, section: null, From, To, [line]);

        var csv = VatRegisterCsvBuilder.Build(report);
        var text = Encoding.UTF8.GetString(csv).TrimStart('\uFEFF');
        byte[] bom = [0xEF, 0xBB, 0xBF];

        Assert.Equal(bom, csv.Take(3).ToArray());
        Assert.Contains("1234.50;22.00;271.59", text, StringComparison.Ordinal);
        Assert.Contains("\"Rossi \"\"Srl\"\"\"", text, StringComparison.Ordinal);

        var seal = VatRegisterCsvBuilder.Seal(csv);
        Assert.Equal(64, seal.Length);
        Assert.Equal(seal, VatRegisterCsvBuilder.Seal(csv));
        line.TaxableAmount++;
        Assert.NotEqual(seal, VatRegisterCsvBuilder.Seal(VatRegisterCsvBuilder.Build(report)), StringComparer.Ordinal);
    }

    /// <summary>
    /// The printable table has one row per line, the rate totals and the totals under it.
    /// </summary>
    [Fact]
    public void Table_HasARowPerLineAndTheTotalsInTheFooter()
    {
        var report = VatRegisterReportBuilder.Build(VatRegisterKind.Sales, section: null, From, To, [Line(1, 22m, 100m, 22m), Line(2, 10m, 100m, 10m)]);

        var table = VatRegisterTable.Build(report, static key => key, "Alpha Srl");

        Assert.Equal("VatCompliance_Kind_Sales", table.Title);
        Assert.Equal(9, table.Headers.Count);
        Assert.Equal(2, table.Rows.Count);
        Assert.StartsWith("Ordinary/1", table.Rows[0][0], StringComparison.Ordinal);
        Assert.Equal(3, table.Footer.Count);
    }

    private static VatRegisterEntry Line(int protocol, decimal rate, decimal taxable, decimal vat, decimal deductible = 0m, decimal nonDeductible = 0m, string? nature = null) => new()
    {
        Kind = VatRegisterKind.Sales,
        Section = VatRegisterSection.Ordinary,
        Year = 2027,
        ProtocolNumber = protocol,
        RegistrationDate = From.AddDays(protocol),
        DocumentDate = From.AddDays(protocol),
        DocumentNumber = "D" + protocol.ToString(System.Globalization.CultureInfo.InvariantCulture),
        VatRate = rate,
        TaxableAmount = taxable,
        VatAmount = vat,
        DeductibleVat = deductible,
        NonDeductibleVat = nonDeductible,
        VatNature = nature,
    };
}
