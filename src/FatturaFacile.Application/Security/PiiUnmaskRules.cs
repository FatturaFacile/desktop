// <copyright file="PiiUnmaskRules.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Security;

/// <summary>
/// Validation rules of the PII unmasking confirmation.
/// </summary>
public static class PiiUnmaskRules
{
    /// <summary>
    /// The minimum length of the justification (after trimming).
    /// </summary>
    public const int MinimumJustificationLength = 10;

    /// <summary>
    /// Checks a justification.
    /// </summary>
    /// <param name="justification">The text typed by the user.</param>
    /// <returns>True when the trimmed text has at least <see cref="MinimumJustificationLength"/> characters.</returns>
    public static bool IsJustificationValid(string? justification) =>
        (justification?.Trim().Length ?? 0) >= MinimumJustificationLength;
}
