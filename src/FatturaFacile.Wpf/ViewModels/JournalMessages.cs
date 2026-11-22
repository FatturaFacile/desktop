// <copyright file="JournalMessages.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Interfaces;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Turns the ledger rule violations into messages in the language of the user.
/// </summary>
internal static class JournalMessages
{
    /// <summary>
    /// Describes a failure; a double-entry violation gets its localized rule text, anything else its own message.
    /// </summary>
    /// <param name="exception">The failure.</param>
    /// <param name="localization">The localization service.</param>
    /// <returns>The message.</returns>
    public static string Describe(Exception exception, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(localization);

        return exception is JournalValidationException { Violations.Count: > 0 } journal
            ? string.Join(' ', journal.Violations.Select(v => localization["Gl_Violation_" + v]))
            : exception.Message;
    }
}
