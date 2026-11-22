// <copyright file="TemplateDefinition.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Accounting;

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// A parametric journal template (causale contabile) ready to be hydrated.
/// </summary>
/// <param name="Id">The template identifier.</param>
/// <param name="Code">The template code.</param>
/// <param name="Name">The name in the current language.</param>
/// <param name="Ledger">The ledger book the template posts to.</param>
/// <param name="Parameters">The parameters asked from the user.</param>
/// <param name="Lines">The lines.</param>
public sealed record TemplateDefinition(
    Guid Id,
    string Code,
    string Name,
    LedgerBook Ledger,
    IReadOnlyList<TemplateParameter> Parameters,
    IReadOnlyList<TemplateLineDefinition> Lines)
{
    /// <summary>
    /// Gets a value indicating whether the template needs a tax rate.
    /// </summary>
    public bool NeedsRate => Parameters.Any(static p => string.Equals(p.Type, "rate", StringComparison.Ordinal));

    /// <summary>
    /// Gets the keys of the accounts the user has to choose.
    /// </summary>
    public IReadOnlyList<string> AccountKeys => [.. Lines.Where(static l => l.IsParameter).Select(static l => l.AccountKey).Distinct(StringComparer.Ordinal)];
}
