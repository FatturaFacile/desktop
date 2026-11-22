// <copyright file="TemplateLineDefinition.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// A line of a template.
/// </summary>
/// <param name="Account">A fixed account code, or "$key" to take the account from the parameter of that key.</param>
/// <param name="Side">The side, debit or credit.</param>
/// <param name="Amount">The amount the line carries: amount (net), tax or gross.</param>
/// <param name="Description">The optional line description.</param>
public sealed record TemplateLineDefinition(string Account, string Side, string Amount, string? Description)
{
    /// <summary>
    /// Gets the key the account is resolved by: the parameter key without its "$", or the fixed code.
    /// </summary>
    public string AccountKey => Account.TrimStart('$');

    /// <summary>
    /// Gets a value indicating whether the account comes from a parameter.
    /// </summary>
    public bool IsParameter => Account.StartsWith('$', StringComparison.Ordinal);
}
