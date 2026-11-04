using FluentAssertions;
using FatturaFacile.Application.Shared.Context;
using FatturaFacile.Application.Shared.Localization;
using FatturaFacile.Domain.Shared.Errors;
using Xunit;

namespace FatturaFacile.Application.Tests;

public class LocalizationTests
{
    private readonly ErrorLocalizer _localizer;
    private readonly CurrentUserContext _userContext;

    public LocalizationTests()
    {
        _userContext = new CurrentUserContext();
        _localizer = new ErrorLocalizer(_userContext);
    }

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
        message.Should().Be(expectedMessage);
    }

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
        message.Should().Be(expectedMessage);
    }

    [Fact]
    public void Localize_AmbientContextCulture_IsRespected()
    {
        // Arrange
        _userContext.Culture = "de-CH";

        // Act
        var message = _localizer.Localize("Tenant.AccessDenied");

        // Assert
        message.Should().Be("Zugriff auf die angeforderte Mandantenressource verweigert.");
    }
}
