// <copyright file="ReceivablesLocalizationTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests that the receivables texts exist in all four resource files with matching placeholders.
/// </summary>
public sealed partial class ReceivablesLocalizationTests
{
    private static readonly string[] Files = ["Strings.resx", "Strings.de.resx", "Strings.fr.resx", "Strings.en.resx"];

    [GeneratedRegex(@"\{\d+\}", RegexOptions.ExplicitCapture, 1000)]
    private static partial Regex PlaceholderPattern { get; }

    /// <summary>
    /// The four resource files carry exactly the same receivables keys, none of them empty.
    /// </summary>
    [Fact]
    public void EveryLocaleFile_HasEveryReceivablesKey()
    {
        var reference = Receivables(Files[0]);

        Assert.True(reference.Count > 150);
        foreach (var file in Files.Skip(1))
        {
            var other = Receivables(file);
            Assert.Equal(reference.Keys.Order(StringComparer.Ordinal), other.Keys.Order(StringComparer.Ordinal));
            Assert.All(other, pair => Assert.False(string.IsNullOrWhiteSpace(pair.Value), file + " " + pair.Key));
        }
    }

    /// <summary>
    /// A translation uses the same {n} placeholders as the Italian text, so a format call never fails in another language.
    /// </summary>
    [Fact]
    public void EveryLocaleFile_KeepsThePlaceholdersOfTheItalianText()
    {
        var reference = Receivables(Files[0]);
        var placeholder = PlaceholderPattern;

        foreach (var file in Files.Skip(1))
        {
            var other = Receivables(file);
            foreach (var (key, text) in reference)
            {
                var expected = placeholder.Matches(text).Select(match => match.Value).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal);
                var actual = placeholder.Matches(other[key]).Select(match => match.Value).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal);
                Assert.Equal(expected, actual);
            }
        }
    }

    /// <summary>
    /// The resources carry a text for every enum value the views show.
    /// </summary>
    [Fact]
    public void ReceivablesKeys_CoverEveryBucketStatusTypePolicyAndScope()
    {
        var keys = Receivables(Files[0]).Keys.ToHashSet(StringComparer.Ordinal);
        string[] wire = ["open", "partially_paid", "paid", "overdue", "disputed", "written_off"];

        Assert.All(wire, name => Assert.Contains("Receivables_Status_" + name, keys));
        Assert.All(new[] { "payment", "credit_note", "debit_adjustment", "write_off", "discount" }, name => Assert.Contains("Receivables_Type_" + name, keys));
        Assert.All(new[] { "warning", "block_order", "block_delivery_note", "credit_hold" }, name => Assert.Contains("Receivables_Policy_" + name, keys));
        Assert.All(new[] { "all", "order", "delivery_note" }, name => Assert.Contains("Receivables_Scope_" + name, keys));
        Assert.All(new[] { "ok", "warning", "block" }, name => Assert.Contains("Receivables_Outcome_" + name, keys));
        Assert.All(new[] { "low", "medium", "high", "critical" }, name => Assert.Contains("Receivables_Rating_" + name, keys));
        Assert.All(Enum.GetNames<FatturaFacile.Domain.Enums.ReceivablesAction>(), name => Assert.Contains("Receivables_Action_" + name, keys));
    }

    private static string ResxDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src", "FatturaFacile.Wpf", "Localization")))
        {
            directory = directory.Parent;
        }

        return directory is null
            ? throw new DirectoryNotFoundException("The localization folder was not found above the test output.")
            : Path.Combine(directory.FullName, "src", "FatturaFacile.Wpf", "Localization");
    }

    private static Dictionary<string, string> Receivables(string file) =>
        XDocument.Load(Path.Combine(ResxDirectory(), file)).Root!
            .Elements("data")
            .Where(data => ((string?)data.Attribute("name"))!.StartsWith("Receivables_", StringComparison.Ordinal) || string.Equals((string?)data.Attribute("name"), "Nav_Receivables", StringComparison.Ordinal))
            .ToDictionary(data => (string)data.Attribute("name")!, data => (string?)data.Element("value") ?? string.Empty, StringComparer.Ordinal);
}
