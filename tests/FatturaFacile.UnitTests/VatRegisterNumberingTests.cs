// <copyright file="VatRegisterNumberingTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the progressive numbering of the VAT registers, the gaps and the pagination.
/// </summary>
public sealed class VatRegisterNumberingTests
{
    /// <summary>
    /// The numbers are progressive in order of registration, per register, sezionale and year, and the rate lines of one document
    /// share one number.
    /// </summary>
    [Fact]
    public void Assign_NumbersEachSeriesInOrderAndKeepsTheDocumentsTogether()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var credit = Guid.NewGuid();
        List<VatRegisterEntry> drafts =
        [
            Entry(VatRegisterSection.Ordinary, "2027-03-05", "B", second, 22m),
            Entry(VatRegisterSection.Ordinary, "2027-03-01", "A", first, 22m),
            Entry(VatRegisterSection.Ordinary, "2027-03-01", "A", first, 10m),
            Entry(VatRegisterSection.CreditNote, "2027-03-02", "C", credit, 22m),
        ];

        VatRegisterNumbering.Assign(drafts, static (_, _, _) => 0);

        int[] expected = [1, 1, 2, 1];
        int[] actual = [drafts[1].ProtocolNumber, drafts[2].ProtocolNumber, drafts[0].ProtocolNumber, drafts[3].ProtocolNumber];

        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// The numbering continues after the last number already used by the series.
    /// </summary>
    [Fact]
    public void Assign_ContinuesAfterTheLastProtocol()
    {
        List<VatRegisterEntry> drafts = [Entry(VatRegisterSection.Ordinary, "2027-04-01", "D", Guid.NewGuid(), 22m)];

        VatRegisterNumbering.Assign(drafts, static (_, section, year) => section == VatRegisterSection.Ordinary && year == 2027 ? 41 : 0);

        Assert.Equal(42, drafts[0].ProtocolNumber);
    }

    /// <summary>
    /// The gaps between the lowest and the highest protocol are found; repeats are ignored.
    /// </summary>
    [Fact]
    public void FindGaps_ReportsTheMissingNumbers()
    {
        int[] expected = [3, 4, 6];

        Assert.Equal(expected, VatRegisterNumbering.FindGaps([1, 2, 2, 5, 7]).ToArray());
        Assert.Empty(VatRegisterNumbering.FindGaps([4, 5, 5, 6]));
        Assert.Empty(VatRegisterNumbering.FindGaps([]));
    }

    /// <summary>
    /// The pages needed to print a number of lines.
    /// </summary>
    /// <param name="lines">The number of lines.</param>
    /// <param name="pages">The expected pages.</param>
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(40, 1)]
    [InlineData(41, 2)]
    [InlineData(400, 10)]
    public void PagesFor_RoundsUp(int lines, int pages) => Assert.Equal(pages, VatRegisterNumbering.PagesFor(lines));

    private static VatRegisterEntry Entry(VatRegisterSection section, string date, string number, Guid source, decimal rate) => new()
    {
        Kind = VatRegisterKind.Sales,
        Section = section,
        Year = 2027,
        RegistrationDate = DateTime.Parse(date, System.Globalization.CultureInfo.InvariantCulture),
        DocumentNumber = number,
        SourceType = VatRegisterBuilder.InvoiceSource,
        SourceId = source,
        VatRate = rate,
    };
}
