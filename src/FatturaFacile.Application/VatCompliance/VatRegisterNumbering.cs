// <copyright file="VatRegisterNumbering.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// The progressive numbering of the VAT registers: one series per register, sezionale and year, in order of registration.
/// </summary>
public static class VatRegisterNumbering
{
    /// <summary>
    /// The number of register lines that fit on one printed page.
    /// </summary>
    public const int LinesPerPage = 40;

    /// <summary>
    /// Assigns the protocol numbers to new lines in order of registration date, then document number. The lines of one document
    /// share one number.
    /// </summary>
    /// <param name="drafts">The new lines.</param>
    /// <param name="lastProtocol">Reads the last protocol already used by a register, section and year; 0 when none.</param>
    public static void Assign(IReadOnlyList<VatRegisterEntry> drafts, Func<VatRegisterKind, VatRegisterSection, int, int> lastProtocol)
    {
        ArgumentNullException.ThrowIfNull(drafts);
        ArgumentNullException.ThrowIfNull(lastProtocol);

        Dictionary<(VatRegisterKind, VatRegisterSection, int), int> counters = [];
        Dictionary<(VatRegisterKind, VatRegisterSection, int, string, Guid), int> documents = [];

        foreach (var entry in drafts.OrderBy(static e => e.RegistrationDate).ThenBy(static e => e.DocumentNumber, StringComparer.Ordinal).ThenBy(static e => e.VatRate))
        {
            var series = (entry.Kind, entry.Section, entry.Year);
            var document = (entry.Kind, entry.Section, entry.Year, entry.SourceType, entry.SourceId ?? entry.Id);

            if (!documents.TryGetValue(document, out var number))
            {
                number = counters.TryGetValue(series, out var current) ? current + 1 : lastProtocol(entry.Kind, entry.Section, entry.Year) + 1;
                counters[series] = number;
                documents[document] = number;
            }

            entry.ProtocolNumber = number;
        }
    }

    /// <summary>
    /// Finds the missing numbers between the lowest and the highest protocol of a series.
    /// </summary>
    /// <param name="protocols">The protocol numbers found, with repeats for documents with several rates.</param>
    /// <returns>The missing numbers in ascending order.</returns>
    public static IReadOnlyList<int> FindGaps(IEnumerable<int> protocols)
    {
        ArgumentNullException.ThrowIfNull(protocols);

        var present = protocols.ToHashSet();

        return present.Count == 0 ? [] : [.. Enumerable.Range(present.Min(), present.Max() - present.Min() + 1).Where(number => !present.Contains(number))];
    }

    /// <summary>
    /// Gets the number of pages needed to print a number of lines.
    /// </summary>
    /// <param name="lines">The number of lines.</param>
    /// <returns>The pages, at least one.</returns>
    public static int PagesFor(int lines) => Math.Max(1, (lines + LinesPerPage - 1) / LinesPerPage);
}
