// <copyright file="FieldServiceValidation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// The checks a field operation passes before it enters the outbox. They mirror the validation rules of the web-api, so an
/// operation the server would reject is refused on the spot instead of becoming a conflict hours later.
/// </summary>
public static partial class FieldServiceValidation
{
    /// <summary>
    /// The localization key of a quantity or a price that is not valid.
    /// </summary>
    public const string QuantityKey = "FieldService_Err_Quantity";

    /// <summary>
    /// The localization key of a lot number that is too long.
    /// </summary>
    public const string LotKey = "FieldService_Err_Lot";

    /// <summary>
    /// The localization key of a status the technician cannot set.
    /// </summary>
    public const string StatusKey = "FieldService_Err_Status";

    /// <summary>
    /// The localization key of a missing work summary.
    /// </summary>
    public const string SummaryKey = "FieldService_Err_Summary";

    /// <summary>
    /// The localization key of hours or kilometres out of range.
    /// </summary>
    public const string HoursKey = "FieldService_Err_Hours";

    /// <summary>
    /// The localization key of a missing signer.
    /// </summary>
    public const string SignerKey = "FieldService_Err_Signer";

    /// <summary>
    /// The localization key of a missing or unsafe signature drawing.
    /// </summary>
    public const string SignatureKey = "FieldService_Err_Signature";

    /// <summary>
    /// The localization key of a checklist line the server would not accept.
    /// </summary>
    public const string ChecklistKey = "FieldService_Err_Checklist";

    /// <summary>
    /// The default language of a service report PDF.
    /// </summary>
    public const string DefaultLocale = "it-IT";

    private const int MaxLot = 60;
    private const decimal MaxHours = 999m;
    private const decimal MaxKilometres = 99999m;
    private const int MaxChecklist = 100;

    private static readonly HashSet<string> TechnicianStatuses = new(StringComparer.Ordinal) { "en_route", "on_site" };
    private static readonly HashSet<string> ChecklistResults = new(StringComparer.Ordinal) { "pass", "fail", "na" };
    private static readonly HashSet<string> Locales = new(StringComparer.Ordinal) { "it-IT", "it-CH", "de-CH", "fr-CH", "en" };

    /// <summary>
    /// Gets the language to print on a report: the requested one when the server supports it, Italian otherwise.
    /// </summary>
    /// <param name="locale">The requested language.</param>
    /// <returns>A supported language.</returns>
    public static string SupportedLocale(string? locale) => locale is not null && Locales.Contains(locale) ? locale : DefaultLocale;

    /// <summary>
    /// Checks a status the technician wants to set.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <returns>The localization key of the error, or null when the status is valid.</returns>
    public static string? CheckStatus(string status) => TechnicianStatuses.Contains(status) ? null : StatusKey;

    /// <summary>
    /// Checks a part consumption.
    /// </summary>
    /// <param name="input">The part.</param>
    /// <returns>The localization key of the error, or null when the part is valid.</returns>
    public static string? CheckPart(FieldServicePartInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return (input.Quantity <= 0 || input.UnitPrice < 0, input.LotNumber?.Length > MaxLot) switch
        {
            (true, _) => QuantityKey,
            (false, true) => LotKey,
            _ => null,
        };
    }

    /// <summary>
    /// Checks a signed service report.
    /// </summary>
    /// <param name="input">The report.</param>
    /// <returns>The localization key of the error, or null when the report is valid.</returns>
    public static string? CheckReport(FieldServiceReportInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input.Summary))
        {
            return SummaryKey;
        }

        if (!HoursInRange(input))
        {
            return HoursKey;
        }

        if (string.IsNullOrWhiteSpace(input.SignerName))
        {
            return SignerKey;
        }

        if (!IsSafeSvg(input.SignatureSvg))
        {
            return SignatureKey;
        }

        return ChecklistIsValid(input.Checklist) ? null : ChecklistKey;
    }

    /// <summary>
    /// Checks that a signature drawing is an SVG without script or event handlers, as the server requires.
    /// </summary>
    /// <param name="svg">The drawing.</param>
    /// <returns>True when the drawing is accepted.</returns>
    public static bool IsSafeSvg(string? svg) =>
        !string.IsNullOrWhiteSpace(svg) && SvgRoot().IsMatch(svg) && !UnsafeSvg().IsMatch(svg);

    private static bool HoursInRange(FieldServiceReportInput input) =>
        input.LaborHours is >= 0 and <= MaxHours
        && input.TravelHours is >= 0 and <= MaxHours
        && input.TravelKm is >= 0 and <= MaxKilometres
        && (input.MeterHours is null or >= 0)
        && (input.MeterCycles is null or >= 0);

    private static bool ChecklistIsValid(IReadOnlyList<FieldServiceChecklistItem> checklist) =>
        checklist.Count <= MaxChecklist && checklist.All(line => !string.IsNullOrWhiteSpace(line.Item) && ChecklistResults.Contains(line.Result));

    [GeneratedRegex(@"^\s*<svg[\s>]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex SvgRoot();

    [GeneratedRegex(@"<\s*script|[\s""']on[a-z]+\s*=|javascript:", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex UnsafeSvg();
}
