// <copyright file="PayslipTemplateLabels.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Reporting;

/// <summary>
/// The fixed texts of a standard payslip template in one language.
/// </summary>
public sealed class PayslipTemplateLabels
{
    /// <summary>
    /// Gets the title.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the "period" label.
    /// </summary>
    public string Period { get; init; } = string.Empty;

    /// <summary>
    /// Gets the "paid days" label.
    /// </summary>
    public string PaidDays { get; init; } = string.Empty;

    /// <summary>
    /// Gets the line table headings (code, description, basis, rate, amount).
    /// </summary>
    public IReadOnlyList<string> Columns { get; init; } = [];

    /// <summary>
    /// Gets the "net salary" label.
    /// </summary>
    public string Net { get; init; } = string.Empty;

    /// <summary>
    /// Gets the "payout" label.
    /// </summary>
    public string Payout { get; init; } = string.Empty;

    /// <summary>
    /// Gets the "employer contributions" label.
    /// </summary>
    public string Employer { get; init; } = string.Empty;

    /// <summary>
    /// Gets the "page" label.
    /// </summary>
    public string Page { get; init; } = string.Empty;
}
