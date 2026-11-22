// <copyright file="TemplateIssueKind.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// Why a template cannot be hydrated.
/// </summary>
public enum TemplateIssueKind
{
    /// <summary>
    /// The net amount is missing or not positive.
    /// </summary>
    AmountRequired = 0,

    /// <summary>
    /// The template needs a tax rate and none was given.
    /// </summary>
    RateRequired = 1,

    /// <summary>
    /// An account of the template is not in the chart of accounts.
    /// </summary>
    AccountUnknown = 2,
}
