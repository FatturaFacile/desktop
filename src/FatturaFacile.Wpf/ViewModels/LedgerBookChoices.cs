// <copyright file="LedgerBookChoices.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Accounting;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Builds the localized ledger book selector shared by the Prima Nota and the reports.
/// </summary>
internal static class LedgerBookChoices
{
    /// <summary>
    /// Builds the three choices in the current language.
    /// </summary>
    /// <param name="localization">The localization service.</param>
    /// <returns>Statutory, tax and international.</returns>
    public static IReadOnlyList<LedgerBookChoice> Build(ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(localization);

        return
        [
            new LedgerBookChoice(LedgerBook.Statutory, localization["Gl_LedgerStatutory"]),
            new LedgerBookChoice(LedgerBook.Tax, localization["Gl_LedgerTax"]),
            new LedgerBookChoice(LedgerBook.Ifrs, localization["Gl_LedgerIfrs"]),
        ];
    }
}
