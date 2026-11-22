// <copyright file="PlanningIds.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Reads the identifiers of the resource planning forms: an entry picked from a list wins over an identifier typed in.
/// </summary>
internal static class PlanningIds
{
    /// <summary>
    /// Resolves an identifier from a picked entry or from the text of an identifier field.
    /// </summary>
    /// <param name="pickedKey">The key of the entry picked from a list, empty when none.</param>
    /// <param name="typedText">The text of the identifier field.</param>
    /// <param name="id">The identifier, or null when nothing was picked or typed.</param>
    /// <returns>False when a text was typed that is not an identifier.</returns>
    public static bool TryResolve(string? pickedKey, string? typedText, out Guid? id)
    {
        if (Guid.TryParse(pickedKey, out var picked))
        {
            id = picked;

            return true;
        }

        if (string.IsNullOrWhiteSpace(typedText))
        {
            id = null;

            return true;
        }

        if (Guid.TryParse(typedText.Trim(), out var typed))
        {
            id = typed;

            return true;
        }

        id = null;

        return false;
    }
}
