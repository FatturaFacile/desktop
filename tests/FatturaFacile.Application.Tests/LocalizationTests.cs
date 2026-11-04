// <copyright file="LocalizationTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Shared.Context;
using FatturaFacile.Application.Shared.Localization;
using FatturaFacile.Domain.Shared.Errors;
using FluentAssertions;
using Xunit;

namespace FatturaFacile.Application.Tests;

/// <summary>
/// Unit tests verifying multi-language localization error resolution and fallback behaviors.
/// </summary>
public class LocalizationTests
{
    private readonly ErrorLocalizer _localizer;
    private readonly CurrentUserContext _userContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationTests"/> class.
    /// </summary>
    public LocalizationTests()
    {
        _userContext = new CurrentUserContext();
        _localizer = new ErrorLocalizer(_userContext);
    }

    /// <summary>
    /// Verifies that invalid credentials error codes translate accurately across all supported languages.
    /// </summary>
    /// <param name="culture">The target culture identifier.</param>
    /// <param name="expectedMessage">The expected localized translation.</param>
    [Theory]
    [InlineData("en-US", "Invalid email or password.")]
    [InlineData("it-CH", "Email o password non validi.")]
    [InlineData("de-CH", "Ungültige E-Mail-Adresse oder Passwort.")]
    [InlineData("fr-CH", "Adresse e-mail ou mot de passe invalide.")]
    public void Localize_InvalidCredentials_ProducesCorrectLanguage(string culture, string expectedMessage)
    {
        // Act
        var message = _localizer.Localize("Identity.InvalidCredentials", culture);

        // Assert
        _ = message.Should().Be(expectedMessage);
    }

    /// <summary>
    /// Verifies that strongly typed DomainErrors.Invoice.NotFound translates accurately into each language.
    /// </summary>
    /// <param name="culture">The target culture code.</param>
    /// <param name="expectedMessage">The expected localized translation.</param>
    [Theory]
    [InlineData("en", "The requested invoice could not be found.")]
    [InlineData("it", "La fattura richiesta non è stata trovata.")]
    [InlineData("de", "Die angeforderte Rechnung konnte nicht gefunden werden.")]
    [InlineData("fr", "La facture demandée est introuvable.")]
    public void Localize_InvoiceNotFound_ProducesCorrectLanguage(string culture, string expectedMessage)
    {
        // Act
        var message = _localizer.Localize(DomainErrors.Invoice.NotFound, culture);

        // Assert
        _ = message.Should().Be(expectedMessage);
    }

    /// <summary>
    /// Verifies that ambient user culture is respected when no explicit culture is passed to Localize.
    /// </summary>
    [Fact]
    public void Localize_AmbientContextCulture_IsRespected()
    {
        // Arrange
        _userContext.Culture = "de-CH";

        // Act
        var message = _localizer.Localize("Tenant.AccessDenied");

        // Assert
        _ = message.Should().Be("Zugriff auf die angeforderte Mandantenressource verweigert.");
    }
}
