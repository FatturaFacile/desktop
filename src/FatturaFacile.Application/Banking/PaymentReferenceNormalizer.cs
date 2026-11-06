// <copyright file="PaymentReferenceNormalizer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Banking;

/// <summary>
/// Normalizes payment references for matching (QRR, SCOR and free references compared without blanks, case-insensitive).
/// </summary>
public static class PaymentReferenceNormalizer
{
    /// <summary>
    /// Normalizes a reference.
    /// </summary>
    /// <param name="reference">The reference.</param>
    /// <returns>The normalized reference, or null when empty.</returns>
    public static string? Normalize(string? reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            return null;
        }

        var compact = new string([.. reference.Where(static c => !char.IsWhiteSpace(c))]).ToUpperInvariant();
        return compact.Length == 0 ? null : compact;
    }
}
