// <copyright file="ReceivablesFormat.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Receivables;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Text helpers shared by the receivables view models.
/// </summary>
internal static class ReceivablesFormat
{
    /// <summary>
    /// The state of a result that went well.
    /// </summary>
    public const string Good = "Good";

    /// <summary>
    /// The state of a result that needs attention.
    /// </summary>
    public const string Warn = "Warn";

    /// <summary>
    /// The state of a result that failed.
    /// </summary>
    public const string Bad = "Bad";

    /// <summary>
    /// Formats an amount with two decimals.
    /// </summary>
    /// <param name="value">The amount.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The text.</returns>
    public static string Money(decimal value, CultureInfo culture) => value.ToString("N2", culture);

    /// <summary>
    /// Formats a day.
    /// </summary>
    /// <param name="date">The day.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The short date.</returns>
    public static string Day(DateOnly date, CultureInfo culture) => date.ToString("d", culture);

    /// <summary>
    /// Formats a percentage that may be missing.
    /// </summary>
    /// <param name="value">The percentage.</param>
    /// <param name="culture">The culture.</param>
    /// <param name="none">The text of a missing value.</param>
    /// <returns>The text.</returns>
    public static string Percent(decimal? value, CultureInfo culture, string none) =>
        value is { } percent ? percent.ToString("0.##", culture) + "%" : none;

    /// <summary>
    /// Parses an amount typed by the user.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="culture">The culture.</param>
    /// <param name="value">The amount.</param>
    /// <returns>True when the text is a number.</returns>
    public static bool TryParseAmount(string? text, CultureInfo culture, out decimal value) =>
        decimal.TryParse(text, NumberStyles.Number, culture, out value);

    /// <summary>
    /// Gets the label of an aging bucket: the localized name of the current bucket, otherwise the day range.
    /// </summary>
    /// <param name="bucket">The bucket.</param>
    /// <param name="localization">The localization service.</param>
    /// <returns>The label.</returns>
    public static string BucketLabel(AgingBucket bucket, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(bucket);
        ArgumentNullException.ThrowIfNull(localization);

        var culture = localization.CurrentCulture;

        return string.Equals(bucket.Key, AgingBucketEngine.Current, StringComparison.Ordinal)
            ? localization["Receivables_Bucket_Current"]
            : RangeLabel(bucket, localization, culture);
    }

    /// <summary>
    /// Describes the result of an action for the user.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="localization">The localization service.</param>
    /// <returns>The text and the state that colours it.</returns>
    public static (string Text, string State) Describe(ReceivablesActionResult result, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(localization);

        return result.Status switch
        {
            ReceivablesActionStatus.Sent => (localization["Receivables_Sent"], Good),
            ReceivablesActionStatus.Queued => (localization["Receivables_Queued"], Warn),
            ReceivablesActionStatus.Conflict => (string.IsNullOrWhiteSpace(result.Detail) ? localization["Receivables_Conflict"] : localization["Receivables_Conflict"] + " " + result.Detail, Bad),
            ReceivablesActionStatus.Rejected => Rejection(result, localization),
            _ => Rejection(result, localization),
        };
    }

    /// <summary>
    /// Gets the state that colours a credit decision.
    /// </summary>
    /// <param name="outcome">The outcome.</param>
    /// <returns>The state.</returns>
    public static string OutcomeState(CreditOutcome outcome) => outcome switch
    {
        CreditOutcome.Block => Bad,
        CreditOutcome.Ok => Good,
        CreditOutcome.Warning => Warn,
        _ => Good,
    };

    private static string RangeLabel(AgingBucket bucket, ILocalizationService localization, CultureInfo culture) =>
        bucket.To is { } to
            ? string.Format(culture, localization["Receivables_Bucket_Range"], bucket.From.ToString(culture), to.ToString(culture))
            : string.Format(culture, localization["Receivables_Bucket_Open"], bucket.From.ToString(culture));

    private static (string Text, string State) Rejection(ReceivablesActionResult result, ILocalizationService localization) =>
        (localization[string.IsNullOrWhiteSpace(result.ErrorKey) ? "Receivables_Err_Generic" : result.ErrorKey], Bad);
}
