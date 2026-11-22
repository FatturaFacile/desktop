// <copyright file="CountingText.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.InventoryCounting;

namespace FatturaFacile.Wpf.ViewModels.PhysicalInventory;

/// <summary>
/// Localized labels and number formats shared by the physical inventory view models.
/// </summary>
internal static class CountingText
{
    /// <summary>
    /// The count type values accepted by the web-api, in display order.
    /// </summary>
    public static readonly IReadOnlyList<string> CountTypes = ["full_physical", "cycle_count_abc", "spot_check", "zone_sweep"];

    /// <summary>
    /// Describes a failed call for the status line.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="result">The failed result.</param>
    /// <param name="localization">The localization service.</param>
    /// <returns>The localized message (validation messages are appended).</returns>
    public static string Describe<T>(InventoryCountingResult<T> result, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(localization);

        if (result.IsOffline)
        {
            return localization["Inv_Offline"];
        }

        if (result.IsForbidden)
        {
            return localization["Inv_Forbidden"];
        }

        var fallback = string.IsNullOrWhiteSpace(result.Message) ? localization["Inv_Abc_SaveError"] : result.Message;

        return result.Errors.Count > 0 ? string.Join(' ', result.Errors) : fallback;
    }

    /// <summary>
    /// Gets the localized label of a count session status.
    /// </summary>
    /// <param name="status">The web-api status value.</param>
    /// <param name="localization">The localization service.</param>
    /// <returns>The label, or the raw value when unknown.</returns>
    public static string StatusLabel(string status, ILocalizationService localization) => status switch
    {
        "in_progress" => localization["Inv_Session_StatusInProgress"],
        "pending_approval" => localization["Inv_Session_StatusPendingApproval"],
        "reconciled" => localization["Inv_Session_StatusReconciled"],
        "cancelled" => localization["Inv_Session_StatusCancelled"],
        _ => status,
    };

    /// <summary>
    /// Gets the localized label of a count type.
    /// </summary>
    /// <param name="countType">The web-api count type value.</param>
    /// <param name="localization">The localization service.</param>
    /// <returns>The label, or the raw value when unknown.</returns>
    public static string CountTypeLabel(string countType, ILocalizationService localization) => countType switch
    {
        "full_physical" => localization["Inv_Session_TypeFull"],
        "cycle_count_abc" => localization["Inv_Session_TypeCycle"],
        "spot_check" => localization["Inv_Session_TypeSpot"],
        "zone_sweep" => localization["Inv_Session_TypeZone"],
        _ => countType,
    };

    /// <summary>
    /// Gets the localized label of a plan cadence.
    /// </summary>
    /// <param name="cadence">The web-api cadence value.</param>
    /// <param name="localization">The localization service.</param>
    /// <returns>The label, or the raw value when unknown.</returns>
    public static string CadenceLabel(string? cadence, ILocalizationService localization) => cadence switch
    {
        "monthly" => localization["Inv_Plan_CadenceMonthly"],
        "quarterly" => localization["Inv_Plan_CadenceQuarterly"],
        "semi_annual" => localization["Inv_Plan_CadenceSemiAnnual"],
        "annual" => localization["Inv_Plan_CadenceAnnual"],
        "custom" => localization["Inv_Plan_CadenceCustom"],
        null => string.Empty,
        _ => cadence,
    };

    /// <summary>
    /// Gets the localized label of an approval status.
    /// </summary>
    /// <param name="status">The web-api approval status.</param>
    /// <param name="localization">The localization service.</param>
    /// <returns>The label, or the raw value when unknown.</returns>
    public static string ApprovalLabel(string status, ILocalizationService localization) => status switch
    {
        "pending" => localization["Inv_Recon_Pending"],
        "approved" => localization["Inv_Recon_Approved"],
        "rejected" => localization["Inv_Recon_Rejected"],
        "cancelled" => localization["Inv_Session_StatusCancelled"],
        _ => status,
    };

    /// <summary>
    /// Formats an amount with two decimals in the current culture.
    /// </summary>
    /// <param name="value">The amount.</param>
    /// <returns>The text.</returns>
    public static string Amount(decimal value) => value.ToString("N2", CultureInfo.CurrentCulture);

    /// <summary>
    /// Formats a quantity (up to four decimals) in the current culture; an absent quantity is empty.
    /// </summary>
    /// <param name="value">The quantity, or null.</param>
    /// <returns>The text.</returns>
    public static string Quantity(decimal? value) => value?.ToString("0.####", CultureInfo.CurrentCulture) ?? string.Empty;

    /// <summary>
    /// Formats a signed quantity (up to four decimals) in the current culture.
    /// </summary>
    /// <param name="value">The quantity.</param>
    /// <returns>The text.</returns>
    public static string SignedQuantity(decimal value) => value.ToString("+0.####;-0.####;0", CultureInfo.CurrentCulture);

    /// <summary>
    /// Formats a signed amount with two decimals in the current culture.
    /// </summary>
    /// <param name="value">The amount.</param>
    /// <returns>The text.</returns>
    public static string SignedAmount(decimal value) => value.ToString("+#,##0.00;-#,##0.00;0.00", CultureInfo.CurrentCulture);

    /// <summary>
    /// Tries to read a decimal typed by the user.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="value">The value.</param>
    /// <returns>True when the text is a number.</returns>
    public static bool TryParseDecimal(string text, out decimal value) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value);
}
