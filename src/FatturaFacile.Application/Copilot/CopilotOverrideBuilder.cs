// <copyright file="CopilotOverrideBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// Builds the overrides sent with a confirmation: only fields listed in the editable fields of the proposed action
/// and only when the user actually changed them.
/// </summary>
public static class CopilotOverrideBuilder
{
    /// <summary>
    /// Builds the typed overrides.
    /// </summary>
    /// <param name="action">The proposed action.</param>
    /// <param name="editedValues">The values typed by the user, by dotted path.</param>
    /// <returns>The overrides, or an empty dictionary when nothing changed.</returns>
    public static IReadOnlyDictionary<string, object?> Build(CopilotProposedAction action, IReadOnlyDictionary<string, string> editedValues)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(editedValues);

        var result = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var path in action.EditableFields)
        {
            var original = action.Arguments.FirstOrDefault(a => string.Equals(a.Path, path, StringComparison.Ordinal));

            if (original is null || !editedValues.TryGetValue(path, out var edited) || string.Equals(edited, original.Value, StringComparison.Ordinal))
            {
                continue;
            }

            result[path] = Convert(original.Kind, edited);
        }

        return result;
    }

    private static object Convert(CopilotArgumentKind kind, string text)
    {
        if (kind == CopilotArgumentKind.Number)
        {
            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariant))
            {
                return invariant;
            }

            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out var local))
            {
                return local;
            }
        }
        else if (kind == CopilotArgumentKind.Boolean && bool.TryParse(text, out var flag))
        {
            return flag;
        }

        return text;
    }
}
