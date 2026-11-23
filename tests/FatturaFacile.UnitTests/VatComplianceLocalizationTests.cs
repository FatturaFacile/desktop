// <copyright file="VatComplianceLocalizationTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0006 // Plain string comparisons by design.
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests that the Mod 029 strings exist in the four resource files with the same keys and placeholders, that every key built from an
/// enum value or an error code exists, and that every key the view uses is defined.
/// </summary>
public sealed partial class VatComplianceLocalizationTests
{
    private static readonly string[] Files = ["Strings.resx", "Strings.en.resx", "Strings.de.resx", "Strings.fr.resx"];

    [GeneratedRegex(@"\{\d+\}", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PlaceholderPattern { get; }

    [GeneratedRegex(@"lex:Loc (?<key>VatCompliance_\w+|Nav_VatCompliance)", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ViewKeyPattern { get; }

    /// <summary>
    /// The four resource files have the same Mod 029 keys, none is empty and the placeholders match.
    /// </summary>
    [Fact]
    public void ResourceFiles_HaveTheSameKeysAndPlaceholders()
    {
        var italian = Read("Strings.resx");
        Assert.NotEmpty(italian);

        foreach (var file in Files.Skip(1))
        {
            var other = Read(file);

            Assert.Equal(italian.Keys.Order(StringComparer.Ordinal), other.Keys.Order(StringComparer.Ordinal));
            foreach (var (key, value) in other)
            {
                Assert.False(string.IsNullOrWhiteSpace(value), file + ": " + key);
                Assert.Equal(Placeholders(italian[key]), Placeholders(value));
            }
        }
    }

    /// <summary>
    /// Every key built from an enum value, and every error key of the module, exists.
    /// </summary>
    [Fact]
    public void ResourceFiles_ContainTheKeysBuiltFromCodes()
    {
        List<string> keys = ["Nav_VatCompliance", "VatCompliance_Section_All"];
        keys.AddRange(Enum.GetNames<VatRegisterKind>().Select(name => "VatCompliance_Kind_" + name));
        keys.AddRange(Enum.GetNames<VatRegisterSection>().Select(name => "VatCompliance_Section_" + name));
        keys.AddRange(Enum.GetNames<LipeTransmissionStatus>().Select(name => "VatCompliance_LipeStatus_" + name));
        keys.AddRange(Enum.GetNames<WithholdingCertificateStatus>().Select(name => "VatCompliance_CuStatus_" + name));
        keys.AddRange(
            typeof(VatComplianceErrors)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(static field => field.IsLiteral)
                .Select(static field => field.GetRawConstantValue() as string ?? string.Empty));

        foreach (var file in Files)
        {
            var strings = Read(file);

            foreach (var key in keys)
            {
                Assert.True(strings.ContainsKey(key), file + ": " + key);
            }
        }
    }

    /// <summary>
    /// Every key the view binds is defined in every resource file.
    /// </summary>
    [Fact]
    public void View_UsesOnlyDefinedKeys()
    {
        var xaml = File.ReadAllText(Path.Combine(SourceRoot(), "Views", "VatComplianceView.xaml"));
        var used = ViewKeyPattern.Matches(xaml).Select(static match => match.Groups["key"].Value).Distinct(StringComparer.Ordinal).ToList();
        Assert.NotEmpty(used);

        foreach (var file in Files)
        {
            var strings = Read(file);

            foreach (var key in used)
            {
                Assert.True(strings.ContainsKey(key), file + ": " + key);
            }
        }
    }

    private static string Placeholders(string value) =>
        string.Join(',', PlaceholderPattern.Matches(value).Select(match => match.Value).Distinct().Order(StringComparer.Ordinal));

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
            .Where(data => data.Attribute("name")!.Value.StartsWith("VatCompliance_", StringComparison.Ordinal) || data.Attribute("name")!.Value == "Nav_VatCompliance")
            .ToDictionary(data => data.Attribute("name")!.Value, data => data.Element("value")!.Value, StringComparer.Ordinal);
}
