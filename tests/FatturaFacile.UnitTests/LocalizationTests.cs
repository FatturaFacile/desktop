// <copyright file="LocalizationTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Wpf.Localization;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Unit tests verifying dynamic multi-language localization, fallback heuristics, and live culture switching.
/// </summary>
public sealed class LocalizationTests
{
    /// <summary>
    /// Verifies that the localization service exposes Italian, English, German, and French.
    /// </summary>
    [Fact]
    public void LocalizationService_SupportedCultures_ContainsAllTargetLanguages()
    {
        var service = LocalizationService.Instance;
        var cultures = service.SupportedCultures;

        Assert.NotEmpty(cultures);
        Assert.Contains(cultures, c => string.Equals(c.Name, "it-IT", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(cultures, c => string.Equals(c.Name, "en-US", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(cultures, c => string.Equals(c.Name, "de-CH", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(cultures, c => string.Equals(c.Name, "fr-CH", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifies that switching cultures changes the returned string values immediately.
    /// </summary>
    [Fact]
    public void LocalizationService_SetCulture_SwitchesResourceStringsDynamically()
    {
        var service = LocalizationService.Instance;

        // Set baseline to it-IT first
        service.SetCulture(new CultureInfo("it-IT"));

        var cultureChangedFired = false;
        service.CultureChanged += (_, _) => cultureChangedFired = true;

        // Switch to English
        service.SetCulture(new CultureInfo("en-US"));
        Assert.True(cultureChangedFired);
        Assert.Equal("Sales Invoicing & Tax Documents", service["Invoicing_Title"]);
        Assert.Equal("Lock", service["Shell_Lock"]);
        Assert.Equal("Log Out", service["Shell_Logout"]);

        // Switch to Italian
        service.SetCulture(new CultureInfo("it-IT"));
        Assert.Equal("Fatturazione Attiva & Documenti Fiscali", service["Invoicing_Title"]);
        Assert.Equal("Blocca", service["Shell_Lock"]);
        Assert.Equal("Disconnetti", service["Shell_Logout"]);

        // Switch to German
        service.SetCulture(new CultureInfo("de-CH"));
        Assert.Equal("Sperren", service["Shell_Lock"]);
        Assert.Equal("Abmelden", service["Shell_Logout"]);

        // Switch to French
        service.SetCulture(new CultureInfo("fr-CH"));
        Assert.Equal("Verrouiller", service["Shell_Lock"]);
    }

    /// <summary>
    /// Verifies that an unsupported culture falls back safely to Italian default culture.
    /// </summary>
    [Fact]
    public void LocalizationService_UnsupportedCulture_FallsBackToDefault()
    {
        var service = LocalizationService.Instance;

        // Set to English first
        service.SetCulture(new CultureInfo("en-US"));
        Assert.Equal("en-US", service.CurrentCulture.Name);

        // Set Japanese (unsupported)
        service.SetCulture(new CultureInfo("ja-JP"));

        // Fallback should map to it-IT
        Assert.Equal("it-IT", service.CurrentCulture.Name);
        Assert.Equal("Blocca", service["Shell_Lock"]);
    }
}
