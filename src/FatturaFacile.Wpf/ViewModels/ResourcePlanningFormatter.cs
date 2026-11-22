// <copyright file="ResourcePlanningFormatter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.ResourcePlanning;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Turns the figures and the codes of the resource planning API into the text of the active language.
/// </summary>
/// <param name="localization">The localization service.</param>
internal sealed class ResourcePlanningFormatter(ILocalizationService localization)
{
    private const string NoValue = "-";
    private const string NumberFormat = "0.##";
    private const string PercentFormat = "0.#";

    /// <summary>
    /// Gets the culture numbers and dates are shown in.
    /// </summary>
    public CultureInfo Culture => localization.CurrentCulture;

    /// <summary>
    /// Gets a text, or a dash when it is empty.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The text, or a dash.</returns>
    public static string OrDash(string? text) => string.IsNullOrWhiteSpace(text) ? NoValue : text;

    /// <summary>
    /// Formats a percentage.
    /// </summary>
    /// <param name="value">The percentage.</param>
    /// <returns>The text, for example 87.5%.</returns>
    public string Percent(double value) => value.ToString(PercentFormat, Culture) + "%";

    /// <summary>
    /// Formats a number of hours.
    /// </summary>
    /// <param name="value">The hours.</param>
    /// <returns>The text with the hours unit of the language.</returns>
    public string Hours(decimal value) => string.Format(Culture, localization["ResourcePlanning_HoursFormat"], value.ToString(NumberFormat, Culture));

    /// <summary>
    /// Formats a day.
    /// </summary>
    /// <param name="day">The day.</param>
    /// <returns>The short date of the culture.</returns>
    public string Day(DateOnly day) => day.ToString("d", Culture);

    /// <summary>
    /// Formats a period.
    /// </summary>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <returns>The two days.</returns>
    public string Period(DateOnly from, DateOnly to) => Day(from) + " - " + Day(to);

    /// <summary>
    /// Gets the name of a booking type in the language of the user.
    /// </summary>
    /// <param name="type">The booking type.</param>
    /// <returns>The localized name.</returns>
    public string Booking(ResourceBookingType type) => localization["ResourcePlanning_Booking_" + type];

    /// <summary>
    /// Describes issues of the server in the language of the user; a code without a text is shown as it is.
    /// </summary>
    /// <param name="issues">The warnings or violations.</param>
    /// <param name="group">The group of the localization keys: Error or Warning.</param>
    /// <returns>The texts joined with a separator.</returns>
    public string Issues(IEnumerable<PlanningIssue> issues, string group)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return string.Join("; ", issues.Select(issue => Describe(group, issue.Code)));
    }

    /// <summary>
    /// Describes a problem by its code.
    /// </summary>
    /// <param name="group">The group of the localization keys.</param>
    /// <param name="code">The code.</param>
    /// <returns>The text of the language, or the code when there is none.</returns>
    public string Describe(string group, string code)
    {
        var key = "ResourcePlanning_" + group + "_" + code;
        var text = localization[key];

        return string.Equals(text, key, StringComparison.Ordinal) ? code : text;
    }

    /// <summary>
    /// Describes a refused write.
    /// </summary>
    /// <param name="status">The HTTP status.</param>
    /// <param name="issues">The violations of the server.</param>
    /// <returns>The violations, or a general text with the status.</returns>
    public string Refusal(HttpStatusCode status, IReadOnlyList<PlanningIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        return issues.Count > 0
            ? Issues(issues, "Error")
            : string.Format(Culture, localization["ResourcePlanning_Error_Generic"], ((int)status).ToString(Culture));
    }

    /// <summary>
    /// Describes the outcome of a mutation: its warnings and the overload it causes.
    /// </summary>
    /// <param name="result">The result of the mutation.</param>
    /// <returns>The texts of the warnings and of the overload, joined; empty when there is nothing to say.</returns>
    public string Warnings(AllocationMutationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var parts = new List<string>();
        if (result.Overload.IsOverloaded)
        {
            parts.Add(string.Format(Culture, localization["ResourcePlanning_OverloadWarning"], Percent(result.Overload.PeakLoadPercent)));
        }

        parts.AddRange(result.Warnings.Where(warning => !string.Equals(warning.Code, "overload", StringComparison.Ordinal)).Select(warning => Describe("Warning", warning.Code)));

        return string.Join(" ", parts);
    }

    /// <summary>
    /// Parses a number the user typed.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The number, or null when the text is empty or not a number.</returns>
    public decimal? ParseDecimal(string? text) =>
        decimal.TryParse(text, NumberStyles.AllowDecimalPoint, Culture, out var value) ? value : null;

    /// <summary>
    /// Parses a whole number the user typed.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The number, or null when the text is empty or not a whole number.</returns>
    public int? ParseInt(string? text) =>
        int.TryParse(text, NumberStyles.Integer, Culture, out var value) ? value : null;
}
