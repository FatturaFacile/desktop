// <copyright file="ErrorLocalizer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Shared.Context;
using FatturaFacile.Domain.Shared.Errors;

namespace FatturaFacile.Application.Shared.Localization;

/// <summary>
/// High-performance strongly typed dictionary-backed localizer providing full 4-language support (EN, IT, DE, FR)
/// for domain errors and validation messages with fallback resolution.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ErrorLocalizer"/> class.
/// </remarks>
/// <param name="userContext">The optional ambient user context for resolving culture preferences.</param>
public sealed class ErrorLocalizer(ICurrentUserContext? userContext = null) : IErrorLocalizer
{
    private static readonly Dictionary<string, Dictionary<string, string>> Translations =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] =
                new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Invoice.NotFound"] = "The requested invoice could not be found.",
                    ["Invoice.AlreadyPaid"] = "The invoice has already been fully paid.",
                    ["Invoice.NumberAlreadyExists"] =
                        "An invoice with this number already exists for this tenant.",
                    ["Invoice.CannotCancel"] =
                        "Cannot cancel an invoice that is already fully paid. Issue a Credit Note instead.",
                    ["Identity.InvalidCredentials"] = "Invalid email or password.",
                    ["Identity.UserLockedOut"] =
                        "User account is locked due to multiple failed login attempts. Please try again later.",
                    ["Identity.InactiveUser"] =
                        "User account is inactive. Please contact your system administrator.",
                    ["Identity.InactiveTenant"] = "The tenant account is disabled.",
                    ["Identity.TenantNotFound"] = "The specified tenant does not exist.",
                    ["Identity.UserNotFound"] = "The specified user does not exist.",
                    ["Identity.EmailAlreadyExists"] =
                        "A user with this email address already exists.",
                    ["Identity.InvalidRefreshToken"] =
                        "The provided refresh token is invalid, expired, or has been revoked.",
                    ["Tenant.MissingContext"] =
                        "A valid tenant context is required for this operation.",
                    ["Tenant.AccessDenied"] =
                        "Access to the requested tenant resource is denied.",
                    ["Tenant.Mismatch"] = "Tenant identifier mismatch on entity mutation.",
                    ["Common.ValidationFailed"] = "One or more validation errors occurred.",
                    ["Common.NullValue"] = "A specified value is null.",
                },
            ["it"] =
                new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Invoice.NotFound"] = "La fattura richiesta non è stata trovata.",
                    ["Invoice.AlreadyPaid"] = "La fattura risulta già interamente saldata.",
                    ["Invoice.NumberAlreadyExists"] =
                        "Esiste già una fattura con questo numero per il tenant corrente.",
                    ["Invoice.CannotCancel"] =
                        "Impossibile annullare una fattura già saldata. Emettere una nota di credito.",
                    ["Identity.InvalidCredentials"] = "Email o password non validi.",
                    ["Identity.UserLockedOut"] =
                        "L'account utente è bloccato a causa di molteplici tentativi falliti. Riprova più tardi.",
                    ["Identity.InactiveUser"] =
                        "L'account utente è disattivato. Contatta l'amministratore di sistema.",
                    ["Identity.InactiveTenant"] =
                        "L'account aziendale (tenant) è disabilitato.",
                    ["Identity.TenantNotFound"] = "Il tenant specificato non esiste.",
                    ["Identity.UserNotFound"] = "L'utente specificato non esiste.",
                    ["Identity.EmailAlreadyExists"] =
                        "Esiste già un utente registrato con questo indirizzo email.",
                    ["Identity.InvalidRefreshToken"] =
                        "Il refresh token fornito non è valido, è scaduto o è stato revocato.",
                    ["Tenant.MissingContext"] =
                        "Per questa operazione è richiesto un contesto tenant valido.",
                    ["Tenant.AccessDenied"] = "Accesso alla risorsa aziendale negato.",
                    ["Tenant.Mismatch"] =
                        "Discrepanza dell'identificativo tenant durante la modifica dell'entità.",
                    ["Common.ValidationFailed"] =
                        "Si sono verificati uno o più errori di convalida.",
                    ["Common.NullValue"] = "Un valore specificato è nullo.",
                },
            ["de"] =
                new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Invoice.NotFound"] = "Die angeforderte Rechnung konnte nicht gefunden werden.",
                    ["Invoice.AlreadyPaid"] = "Die Rechnung wurde bereits vollständig bezahlt.",
                    ["Invoice.NumberAlreadyExists"] =
                        "Eine Rechnung mit dieser Nummer existiert bereits für diesen Mandanten.",
                    ["Invoice.CannotCancel"] =
                        "Eine bereits bezahlte Rechnung kann nicht storniert werden. Erstellen Sie eine Gutschrift.",
                    ["Identity.InvalidCredentials"] = "Ungültige E-Mail-Adresse oder Passwort.",
                    ["Identity.UserLockedOut"] =
                        "Benutzerkonto ist wegen zu vieler fehlgeschlagener Anmeldeversuche gesperrt. Bitte versuchen Sie es später erneut.",
                    ["Identity.InactiveUser"] =
                        "Das Benutzerkonto ist inaktiv. Bitte kontaktieren Sie Ihren Administrator.",
                    ["Identity.InactiveTenant"] = "Das Mandantenkonto ist deaktiviert.",
                    ["Identity.TenantNotFound"] = "Der angegebene Mandant existiert nicht.",
                    ["Identity.UserNotFound"] = "Der angegebene Benutzer existiert nicht.",
                    ["Identity.EmailAlreadyExists"] =
                        "Ein Benutzer mit dieser E-Mail-Adresse existiert bereits.",
                    ["Identity.InvalidRefreshToken"] =
                        "Das angegebene Refresh-Token ist ungültig, abgelaufen oder widerrufen.",
                    ["Tenant.MissingContext"] =
                        "Für diesen Vorgang ist ein gültiger Mandantenkontext erforderlich.",
                    ["Tenant.AccessDenied"] = "Zugriff auf die angeforderte Mandantenressource verweigert.",
                    ["Tenant.Mismatch"] = "Mandanten-ID-Konflikt bei der Entitätsänderung.",
                    ["Common.ValidationFailed"] =
                        "Ein oder mehrere Validierungsfehler sind aufgetreten.",
                    ["Common.NullValue"] = "Ein angegebener Wert ist null.",
                },
            ["fr"] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Invoice.NotFound"] = "La facture demandée est introuvable.",
                ["Invoice.AlreadyPaid"] = "La facture a déjà été intégralement réglée.",
                ["Invoice.NumberAlreadyExists"] =
                    "Une facture portant ce numéro existe déjà pour ce locataire.",
                ["Invoice.CannotCancel"] =
                    "Impossible d'annuler une facture déjà payée. Émettez un avoir.",
                ["Identity.InvalidCredentials"] = "Adresse e-mail ou mot de passe invalide.",
                ["Identity.UserLockedOut"] =
                    "Le compte est verrouillé en raison de tentatives de connexion échouées répétées. Veuillez réessayer plus tard.",
                ["Identity.InactiveUser"] =
                    "Le compte utilisateur est inactif. Veuillez contacter votre administrateur.",
                ["Identity.InactiveTenant"] = "Le compte locataire est désactivé.",
                ["Identity.TenantNotFound"] = "Le locataire spécifié n'existe pas.",
                ["Identity.UserNotFound"] = "L'utilisateur spécifié n'existe pas.",
                ["Identity.EmailAlreadyExists"] =
                    "Un utilisateur avec cette adresse e-mail existe déjà.",
                ["Identity.InvalidRefreshToken"] =
                    "Le jeton d'actualisation fourni est invalide, expiré ou révoqué.",
                ["Tenant.MissingContext"] =
                    "Un contexte de locataire valide est requis pour cette opération.",
                ["Tenant.AccessDenied"] = "Accès refusé à la ressource du locataire.",
                ["Tenant.Mismatch"] =
                    "Incohérence de l'identifiant du locataire lors de la modification de l'entité.",
                ["Common.ValidationFailed"] =
                    "Une ou plusieurs erreurs de validation se sont produites.",
                ["Common.NullValue"] = "Une valeur spécifiée est nulle.",
            },
        };

    private readonly ICurrentUserContext? _userContext = userContext;

    /// <inheritdoc />
    public string Localize(string errorCode, string? culture = null, params object[] arguments)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            return string.Empty;
        }

        var targetCulture = culture ?? _userContext?.Culture ?? CultureInfo.CurrentUICulture.Name;
        var lang = GetTwoLetterLanguageCode(targetCulture);

        if (Translations.TryGetValue(lang, out var langDict) &&
            langDict.TryGetValue(errorCode, out var message))
        {
            ArgumentNullException.ThrowIfNull(arguments);
            return arguments.Length > 0 ? string.Format(message, arguments) : message;
        }

        // Fallback to English
        if (Translations["en"].TryGetValue(errorCode, out var enMessage))
        {
            ArgumentNullException.ThrowIfNull(arguments);
            if (arguments.Length > 0)
            {
                // Fallback to English
                return string.Format(enMessage, arguments);
            }

            // Fallback to English
            return enMessage;
        }

        // Fallback to English
        return errorCode;
    }

    /// <inheritdoc />
    public string Localize(DomainErrorDefinition error, string? culture = null)
    {
        ArgumentNullException.ThrowIfNull(error);
        if (error == DomainErrorDefinition.None)
        {
            return string.Empty;
        }

        var localized = Localize(error.Code, culture);
        return string.Equals(localized, error.Code, StringComparison.OrdinalIgnoreCase)
            ? error.Message
            : localized;
    }

    private static string GetTwoLetterLanguageCode(string cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            return "en";
        }

        var parts = cultureName.Split(['-', '_']);
        var prefix = parts[0].ToLowerInvariant();
        return prefix switch
        {
            "it" => "it",
            "de" => "de",
            "fr" => "fr",
            _ => "en",
        };
    }
}
