// <copyright file="MesCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Mes;

/// <summary>
/// A MES operator command (CQRS write side): what the operator did, on which phase and when. The payload holds the web-api field
/// names (snake_case); quantities are decimal strings with a dot.
/// </summary>
/// <param name="Id">The command identifier, sent as idempotency key.</param>
/// <param name="Action">The action.</param>
/// <param name="PhaseId">The server phase, null before a take-over reached the server.</param>
/// <param name="OperationId">The production order operation.</param>
/// <param name="OccurredAtUtc">The instant the operator acted (UTC).</param>
/// <param name="Payload">The fields of the action.</param>
public sealed record MesCommand(Guid Id, MesAction Action, Guid? PhaseId, Guid OperationId, DateTime OccurredAtUtc, IReadOnlyDictionary<string, object?> Payload)
{
    /// <summary>
    /// The payload field of the operator of a take-over.
    /// </summary>
    public const string OperatorField = "operator_id";

    /// <summary>
    /// The payload field of the machine mode.
    /// </summary>
    public const string MachineModeField = "machine_mode";

    /// <summary>
    /// The payload field of the suspension reason.
    /// </summary>
    public const string ReasonField = "reason";

    /// <summary>
    /// The payload field of the good pieces.
    /// </summary>
    public const string GoodField = "good_quantity";

    /// <summary>
    /// The payload field of the scrapped pieces.
    /// </summary>
    public const string ScrapField = "scrap_quantity";

    /// <summary>
    /// The payload field of the defect reason.
    /// </summary>
    public const string DefectField = "defect_reason";

    /// <summary>
    /// The payload field of the consumed item.
    /// </summary>
    public const string ItemField = "mfg_item_id";

    /// <summary>
    /// The payload field of the lot.
    /// </summary>
    public const string LotField = "lot_code";

    /// <summary>
    /// The payload field of the consumed quantity.
    /// </summary>
    public const string QuantityField = "quantity";

    /// <summary>
    /// Creates the take-over of an operation by an operator.
    /// </summary>
    /// <param name="operationId">The operation.</param>
    /// <param name="phaseId">The phase, if the operation already has one.</param>
    /// <param name="operatorId">The operator.</param>
    /// <param name="at">When the operator acted (UTC).</param>
    /// <returns>The command.</returns>
    public static MesCommand TakeOver(Guid operationId, Guid? phaseId, Guid operatorId, DateTime at) =>
        Create(MesAction.TakeOver, phaseId, operationId, at, (OperatorField, operatorId.ToString("D", CultureInfo.InvariantCulture)));

    /// <summary>
    /// Creates the start of a setup.
    /// </summary>
    /// <param name="row">The phase.</param>
    /// <param name="at">When the operator acted (UTC).</param>
    /// <returns>The command.</returns>
    public static MesCommand StartSetup(MesPhaseRow row, DateTime at)
    {
        ArgumentNullException.ThrowIfNull(row);

        return Create(MesAction.StartSetup, row.PhaseId, row.OperationId, at);
    }

    /// <summary>
    /// Creates the start of the machining.
    /// </summary>
    /// <param name="row">The phase.</param>
    /// <param name="mode">Attended or unattended.</param>
    /// <param name="at">When the operator acted (UTC).</param>
    /// <returns>The command.</returns>
    public static MesCommand StartProduction(MesPhaseRow row, MesMachineMode mode, DateTime at)
    {
        ArgumentNullException.ThrowIfNull(row);

        return Create(MesAction.StartProduction, row.PhaseId, row.OperationId, at, (MachineModeField, MesWire.ToWire(mode)));
    }

    /// <summary>
    /// Creates a suspension.
    /// </summary>
    /// <param name="row">The phase.</param>
    /// <param name="reason">The reason.</param>
    /// <param name="at">When the operator acted (UTC).</param>
    /// <returns>The command.</returns>
    public static MesCommand Suspend(MesPhaseRow row, MesSuspensionReason reason, DateTime at)
    {
        ArgumentNullException.ThrowIfNull(row);

        return Create(MesAction.Suspend, row.PhaseId, row.OperationId, at, (ReasonField, MesWire.ToWire(reason)));
    }

    /// <summary>
    /// Creates a resume.
    /// </summary>
    /// <param name="row">The phase.</param>
    /// <param name="at">When the operator acted (UTC).</param>
    /// <returns>The command.</returns>
    public static MesCommand Resume(MesPhaseRow row, DateTime at)
    {
        ArgumentNullException.ThrowIfNull(row);

        return Create(MesAction.Resume, row.PhaseId, row.OperationId, at);
    }

    /// <summary>
    /// Creates a production declaration.
    /// </summary>
    /// <param name="row">The phase.</param>
    /// <param name="good">The good pieces.</param>
    /// <param name="scrap">The scrapped pieces.</param>
    /// <param name="defect">The defect reason of the scrapped pieces.</param>
    /// <param name="at">When the operator acted (UTC).</param>
    /// <returns>The command.</returns>
    public static MesCommand Declare(MesPhaseRow row, decimal good, decimal scrap, MesDefectReason? defect, DateTime at)
    {
        ArgumentNullException.ThrowIfNull(row);

        return Create(
            MesAction.DeclareProduction,
            row.PhaseId,
            row.OperationId,
            at,
            (GoodField, Quantity(good)),
            (ScrapField, Quantity(scrap)),
            (DefectField, defect is { } reason ? MesWire.ToWire(reason) : null));
    }

    /// <summary>
    /// Creates the registration of a consumed lot.
    /// </summary>
    /// <param name="row">The phase.</param>
    /// <param name="itemId">The component.</param>
    /// <param name="lotCode">The lot, as scanned or typed.</param>
    /// <param name="quantity">The consumed quantity.</param>
    /// <param name="overConsumptionConfirmed">Whether the operator confirmed consuming more than required.</param>
    /// <param name="at">When the operator acted (UTC).</param>
    /// <returns>The command.</returns>
    public static MesCommand ConsumeMaterial(MesPhaseRow row, Guid itemId, string? lotCode, decimal quantity, bool overConsumptionConfirmed, DateTime at)
    {
        ArgumentNullException.ThrowIfNull(row);

        return Create(
            MesAction.ConsumeMaterial,
            row.PhaseId,
            row.OperationId,
            at,
            (ItemField, itemId.ToString("D", CultureInfo.InvariantCulture)),
            (LotField, string.IsNullOrWhiteSpace(lotCode) ? null : lotCode.Trim()),
            (QuantityField, Quantity(quantity)),
            ("over_consumption_confirmed", overConsumptionConfirmed));
    }

    /// <summary>
    /// Creates the closing of a phase.
    /// </summary>
    /// <param name="row">The phase.</param>
    /// <param name="at">When the operator acted (UTC).</param>
    /// <returns>The command.</returns>
    public static MesCommand Close(MesPhaseRow row, DateTime at)
    {
        ArgumentNullException.ThrowIfNull(row);

        return Create(MesAction.Close, row.PhaseId, row.OperationId, at);
    }

    /// <summary>
    /// Formats a quantity the way the web-api expects it.
    /// </summary>
    /// <param name="value">The quantity.</param>
    /// <returns>The decimal string.</returns>
    public static string Quantity(decimal value) => value.ToString("0.####", CultureInfo.InvariantCulture);

    /// <summary>
    /// Reads a text field of the payload.
    /// </summary>
    /// <param name="name">The field.</param>
    /// <returns>The text, or null when absent.</returns>
    public string? Text(string name) => Payload.TryGetValue(name, out var value) ? Convert.ToString(value, CultureInfo.InvariantCulture) : null;

    /// <summary>
    /// Reads a quantity field of the payload.
    /// </summary>
    /// <param name="name">The field.</param>
    /// <returns>The quantity, zero when absent.</returns>
    public decimal Amount(string name) =>
        decimal.TryParse(Text(name), NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : 0m;

    private static MesCommand Create(MesAction action, Guid? phaseId, Guid operationId, DateTime at, params (string Name, object? Value)[] fields) =>
        new(Guid.NewGuid(), action, phaseId, operationId, at, fields.ToDictionary(field => field.Name, field => field.Value, StringComparer.Ordinal));
}
