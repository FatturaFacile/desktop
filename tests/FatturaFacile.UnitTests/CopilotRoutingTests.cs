// <copyright file="CopilotRoutingTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.

using System.Globalization;
using FatturaFacile.Application.Copilot;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the mapping of citations to native modules and of UI cultures to API locales.
/// </summary>
public sealed class CopilotRoutingTests
{
    /// <summary>
    /// Entity citations resolve to a native module by entity type, then by route segment.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="route">The route.</param>
    /// <param name="expected">The expected module.</param>
    [Theory]
    [InlineData("invoice", "/invoices/42", "Invoicing")]
    [InlineData("Customer", null, "Contacts")]
    [InlineData("product", "/products/7", "Products")]
    [InlineData(null, "/contacts/9", "Contacts")]
    [InlineData("unknown_thing", "/payroll/5?tab=1", "Payroll")]
    [InlineData("unknown_thing", "/nowhere", null)]
    public void ResolveModule_MapsToNativeModules(string? entityType, string? route, string? expected)
        => Assert.Equal(expected, CopilotRouteResolver.ResolveModule(entityType, route));

    /// <summary>
    /// UI cultures map to locales accepted by the API.
    /// </summary>
    /// <param name="culture">The culture name.</param>
    /// <param name="expected">The API locale.</param>
    [Theory]
    [InlineData("it-IT", "it-IT")]
    [InlineData("de-CH", "de-CH")]
    [InlineData("fr-CH", "fr-CH")]
    [InlineData("en-US", "en")]
    [InlineData("de-DE", "de")]
    [InlineData("ja-JP", "en")]
    public void Locale_MapsCultureToApiLocale(string culture, string expected)
        => Assert.Equal(expected, CopilotLocale.From(CultureInfo.GetCultureInfo(culture)));
}
