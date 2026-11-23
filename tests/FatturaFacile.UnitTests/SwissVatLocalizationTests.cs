// <copyright file="SwissVatLocalizationTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using FatturaFacile.Application.SwissVat;
using FatturaFacile.Wpf.Localization;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Verifies Mod 024 Swiss VAT resource completeness across it-CH/it-IT, en, de-CH, and fr-CH,
/// plus view key bindings and error constant coverage.
/// </summary>
public sealed partial class SwissVatLocalizationTests
{
    private static readonly string[] Files = ["Strings.resx", "Strings.en.resx", "Strings.de.resx", "Strings.fr.resx"];

    [GeneratedRegex(@"\{\d+\}", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PlaceholderPattern { get; }

    [GeneratedRegex(@"lex:Loc (?<key>SwissVat_\w+|Nav_SwissVat)", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ViewKeyPattern { get; }

    /// <summary>
    /// All four .resx resource files define the exact same SwissVat_ and Nav_SwissVat keys with non-empty values and matching placeholders.
    /// </summary>
    [Fact]
    public void ResourceFiles_HaveSameSwissVatKeysAndPlaceholders()
    {
        var italian = Read("Strings.resx");
        Assert.NotEmpty(italian);

        foreach (var file in Files.Skip(1))
        {
            var other = Read(file);

            Assert.Equal(italian.Keys.Order(StringComparer.Ordinal), other.Keys.Order(StringComparer.Ordinal));
            foreach (var (key, value) in other)
            {
                Assert.False(string.IsNullOrWhiteSpace(value), string.Concat(file, ": ", key));
                Assert.Equal(Placeholders(italian[key]), Placeholders(value));
            }
        }
    }

    /// <summary>
    /// Every SwissVatErrors key and Q1..Q9 quadratura message key exists across all four resource files.
    /// </summary>
    [Fact]
    public void ResourceFiles_ContainAllErrorAndQuadraturaKeys()
    {
        List<string> keys = ["Nav_SwissVat", "SwissVat_Quad_Passed", "SwissVat_Quad_Failed"];
        for (var i = 1; i <= 9; i++)
        {
            keys.Add(string.Concat("SwissVat_Quad_Q", i.ToString(CultureInfo.InvariantCulture)));
        }

        keys.AddRange(
            typeof(SwissVatErrors)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(static f => f.IsLiteral)
                .Select(static f => f.GetRawConstantValue() as string ?? string.Empty));

        foreach (var file in Files)
        {
            var strings = Read(file);
            foreach (var key in keys)
            {
                Assert.True(strings.ContainsKey(key), string.Concat(file, ": ", key));
            }
        }
    }

    /// <summary>
    /// Every key referenced in SwissVatView.xaml is present in all four resource files.
    /// </summary>
    [Fact]
    public void View_UsesOnlyDefinedKeys()
    {
        var xaml = File.ReadAllText(Path.Combine(SourceRoot(), "Views", "SwissVatView.xaml"));
        var used = ViewKeyPattern.Matches(xaml)
            .Select(static m => m.Groups["key"].Value)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        Assert.NotEmpty(used);

        foreach (var file in Files)
        {
            var strings = Read(file);
            foreach (var key in used)
            {
                Assert.True(strings.ContainsKey(key), string.Concat(file, ": ", key));
            }
        }
    }

    /// <summary>
    /// Verifies live localization lookup across it-CH, it-IT, de-CH, fr-CH, and en.
    /// </summary>
    /// <param name="cultureName">Target culture name.</param>
    [Theory]
    [InlineData("it-CH")]
    [InlineData("it-IT")]
    [InlineData("de-CH")]
    [InlineData("fr-CH")]
    [InlineData("en")]
    public void LocalizationService_ResolvesSwissVatTitleForFiveLocales(string cultureName)
    {
        var service = LocalizationService.Instance;
        service.SetCulture(new CultureInfo(cultureName));

        var resolved = service["SwissVat_Title"];
        Assert.False(string.IsNullOrWhiteSpace(resolved));
        Assert.NotEqual("SwissVat_Title", resolved);
    }

    private static string Placeholders(string value) =>
        string.Join(',', PlaceholderPattern.Matches(value).Select(static match => match.Value).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal));

    private static string SourceRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "FatturaFacile.slnx")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return Path.Combine(directory.FullName, "src", "FatturaFacile.Wpf");
    }

    private static Dictionary<string, string> Read(string file) =>
        XDocument.Load(Path.Combine(SourceRoot(), "Localization", file)).Root!
            .Elements("data")
            .Where(static data => data.Attribute("name")!.Value.StartsWith("SwissVat_", StringComparison.Ordinal) || string.Equals(data.Attribute("name")!.Value, "Nav_SwissVat", StringComparison.Ordinal))
            .ToDictionary(static data => data.Attribute("name")!.Value, static data => data.Element("value")!.Value, StringComparer.Ordinal);
}
