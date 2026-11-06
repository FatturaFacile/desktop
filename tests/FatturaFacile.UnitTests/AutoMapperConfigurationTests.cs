// <copyright file="AutoMapperConfigurationTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Unit tests verifying AutoMapper profile mappings and configuration integrity.
/// </summary>
public sealed class AutoMapperConfigurationTests
{
    /// <summary>
    /// Verifies that all AutoMapper mappings are valid and conform to strict mapping expectations.
    /// </summary>
    [Fact]
    public void AutoMapper_Configuration_IsValid()
    {
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddApplicationServices();
        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}
