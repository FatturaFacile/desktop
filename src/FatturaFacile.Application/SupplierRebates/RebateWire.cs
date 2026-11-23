// <copyright file="RebateWire.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// Maps the enums of the supplier rebates to and from the codes of the web-api.
/// </summary>
public static class RebateWire
{
    private static readonly Dictionary<string, SettlementStatus> StatusCodes = new(StringComparer.Ordinal)
    {
        ["draft"] = SettlementStatus.Draft,
        ["pending_approval"] = SettlementStatus.PendingApproval,
        ["approved"] = SettlementStatus.Approved,
        ["settled"] = SettlementStatus.Settled,
        ["cancelled"] = SettlementStatus.Cancelled,
    };

    private static readonly Dictionary<string, RebateExecutionMode> ModeCodes = new(StringComparer.Ordinal)
    {
        ["credit_note_request"] = RebateExecutionMode.CreditNoteRequest,
        ["charge_invoice"] = RebateExecutionMode.ChargeInvoice,
        ["ap_set_off"] = RebateExecutionMode.ApSetOff,
    };

    /// <summary>
    /// Reads a tier mode code; an unknown code is retroactive, the default of the server.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <returns>The tier mode.</returns>
    public static RebateTierMode ParseTierMode(string? code) =>
        string.Equals(code, RebateCodes.ModeIncremental, StringComparison.Ordinal) ? RebateTierMode.Incremental : RebateTierMode.Retroactive;

    /// <summary>
    /// Reads a settlement status code; an unknown code is a draft.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <returns>The status.</returns>
    public static SettlementStatus ParseStatus(string? code) => code is not null && StatusCodes.TryGetValue(code, out var status) ? status : SettlementStatus.Draft;

    /// <summary>
    /// Reads an execution mode code.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <returns>The mode, or null when the code is missing or unknown.</returns>
    public static RebateExecutionMode? ParseMode(string? code) => code is not null && ModeCodes.TryGetValue(code, out var mode) ? mode : null;

    /// <summary>
    /// Gets the path segment of a settlement action.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <returns>The segment: submit, approve, execute or cancel.</returns>
    public static string ToWire(SettlementAction action) => action switch
    {
        SettlementAction.Submit => "submit",
        SettlementAction.Approve => "approve",
        SettlementAction.Execute => "execute",
        SettlementAction.Cancel => "cancel",
        _ => "cancel",
    };

    /// <summary>
    /// Gets the code of a tier mode.
    /// </summary>
    /// <param name="mode">The tier mode.</param>
    /// <returns>The code.</returns>
    public static string ToWire(RebateTierMode mode) => mode == RebateTierMode.Incremental ? RebateCodes.ModeIncremental : RebateCodes.ModeRetroactive;

    /// <summary>
    /// Gets the code of a settlement status.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <returns>The code.</returns>
    public static string ToWire(SettlementStatus status) => StatusCodes.First(pair => pair.Value == status).Key;

    /// <summary>
    /// Gets the code of an execution mode.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <returns>The code.</returns>
    public static string ToWire(RebateExecutionMode mode) => ModeCodes.First(pair => pair.Value == mode).Key;
}
