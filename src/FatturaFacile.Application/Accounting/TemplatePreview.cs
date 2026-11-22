// <copyright file="TemplatePreview.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// The lines a template yields for the given inputs; nothing is written.
/// </summary>
/// <param name="Lines">The hydrated lines.</param>
/// <param name="Issues">The problems; when not empty the lines must not be used.</param>
public sealed record TemplatePreview(IReadOnlyList<JournalLineInput> Lines, IReadOnlyList<TemplateIssue> Issues)
{
    /// <summary>
    /// Gets the debit and credit totals.
    /// </summary>
    public JournalTotals Totals => JournalRules.Totals(Lines);

    /// <summary>
    /// Gets a value indicating whether the template could be hydrated into balanced lines.
    /// </summary>
    public bool IsBalanced => Issues.Count == 0 && Totals.IsBalanced;
}
