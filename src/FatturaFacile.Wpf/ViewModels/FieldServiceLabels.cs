// <copyright file="FieldServiceLabels.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.FieldService;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Turns the values of the Field Service read models into the localized, culture formatted text the views show. The statuses,
/// types and priorities of the server are open sets, so an unknown value is shown as received instead of as a missing key.
/// </summary>
/// <param name="localization">The localization service.</param>
internal sealed class FieldServiceLabels(ILocalizationService localization)
{
    private const string NoValue = "-";
    private const string MoneyFormat = "N2";
    private const string NumberFormat = "0.##";
    private const string MomentFormat = "g";
    private const string DayFormat = "d";

    /// <summary>
    /// Gets the culture used to format numbers and dates.
    /// </summary>
    public CultureInfo Culture => localization.CurrentCulture;

    /// <summary>
    /// Gets the localized name of a work order status.
    /// </summary>
    /// <param name="status">The status of the server.</param>
    /// <returns>The label.</returns>
    public string Status(string status) => Pick("FieldService_Status_", status);

    /// <summary>
    /// Gets the localized name of a work type.
    /// </summary>
    /// <param name="workType">The work type of the server.</param>
    /// <returns>The label.</returns>
    public string WorkType(string workType) => Pick("FieldService_Type_", workType);

    /// <summary>
    /// Gets the localized name of a priority.
    /// </summary>
    /// <param name="priority">The priority of the server.</param>
    /// <returns>The label.</returns>
    public string Priority(string priority) => Pick("FieldService_Prio_", priority);

    /// <summary>
    /// Gets the localized name of a contract status.
    /// </summary>
    /// <param name="status">The status of the server.</param>
    /// <returns>The label.</returns>
    public string ContractStatus(string status) => Pick("FieldService_ContractStatus_", status);

    /// <summary>
    /// Gets the localized name of a coverage type.
    /// </summary>
    /// <param name="coverage">The coverage type.</param>
    /// <returns>The label.</returns>
    public string Coverage(MaintenanceCoverageType coverage) => localization["FieldService_Coverage_" + coverage.ToString()];

    /// <summary>
    /// Gets the localized name of a billing line kind.
    /// </summary>
    /// <param name="kind">The kind of the server (labor, travel, callout, part).</param>
    /// <returns>The label.</returns>
    public string BillingKind(string kind) => Pick("FieldService_Kind_", kind);

    /// <summary>
    /// Formats an amount with its currency.
    /// </summary>
    /// <param name="value">The amount.</param>
    /// <param name="currency">The currency, possibly empty.</param>
    /// <returns>The text.</returns>
    public string Money(decimal value, string currency) =>
        (string.IsNullOrEmpty(currency) ? string.Empty : currency + " ") + value.ToString(MoneyFormat, Culture);

    /// <summary>
    /// Formats a quantity or a number of hours without trailing zeros.
    /// </summary>
    /// <param name="value">The number.</param>
    /// <returns>The text.</returns>
    public string Number(decimal value) => value.ToString(NumberFormat, Culture);

    /// <summary>
    /// Formats an instant in the local time zone.
    /// </summary>
    /// <param name="moment">The instant, or null.</param>
    /// <returns>The text, or a dash.</returns>
    public string Moment(DateTimeOffset? moment) => moment is { } value ? value.ToLocalTime().ToString(MomentFormat, Culture) : NoValue;

    /// <summary>
    /// Formats a date.
    /// </summary>
    /// <param name="day">The date, or null.</param>
    /// <returns>The text, or a dash.</returns>
    public string Day(DateOnly? day) => day is { } value ? value.ToString(DayFormat, Culture) : NoValue;

    /// <summary>
    /// Formats the window of a scheduled visit.
    /// </summary>
    /// <param name="start">The start, or null.</param>
    /// <param name="end">The end, or null.</param>
    /// <returns>The text, or a dash when unscheduled.</returns>
    public string Window(DateTimeOffset? start, DateTimeOffset? end) =>
        start is null ? NoValue : Moment(start) + " - " + Moment(end);

    /// <summary>
    /// Formats a localized message with arguments.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <param name="arguments">The arguments.</param>
    /// <returns>The text.</returns>
    public string Format(string key, params string[] arguments) => string.Format(Culture, localization[key], arguments);

    /// <summary>
    /// Gets the message of an online call.
    /// </summary>
    /// <param name="status">The outcome.</param>
    /// <param name="detail">The error detail.</param>
    /// <returns>The localized message.</returns>
    public string Call(FieldServiceCallStatus status, string? detail) => status switch
    {
        FieldServiceCallStatus.Ok => localization["FieldService_Synced"],
        FieldServiceCallStatus.Offline => localization["FieldService_OfflineUsingCache"],
        FieldServiceCallStatus.Unauthorized => localization["FieldService_SignInRequired"],
        FieldServiceCallStatus.Forbidden => localization["FieldService_Forbidden"],
        FieldServiceCallStatus.Rejected => Format("FieldService_Rejected", detail ?? string.Empty),
        _ => Format("FieldService_Failed", detail ?? string.Empty),
    };

    /// <summary>
    /// Gets the localized text of a dispatch warning.
    /// </summary>
    /// <param name="warning">The warning.</param>
    /// <returns>The text.</returns>
    public string Warning(FieldServiceWarning warning)
    {
        ArgumentNullException.ThrowIfNull(warning);

        return warning.Kind switch
        {
            FieldServiceWarningKind.Schedule => Format("FieldService_Warn_Schedule", warning.Detail),
            FieldServiceWarningKind.Skills => Format("FieldService_Warn_Skills", warning.Detail),
            FieldServiceWarningKind.VanStock => localization["FieldService_Warn_Van"],
            _ => warning.Detail,
        };
    }

    /// <summary>
    /// Gets a localized message for a key, or the key itself when the validation message is not a known key.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <returns>The text.</returns>
    public string Key(string key) => localization[key];

    private string Pick(string prefix, string value)
    {
        var key = prefix + value;
        var text = localization[key];

        return string.Equals(text, key, StringComparison.Ordinal) ? value : text;
    }
}
