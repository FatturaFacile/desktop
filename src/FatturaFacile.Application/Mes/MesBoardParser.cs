// <copyright file="MesBoardParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Mes;

/// <summary>
/// Parses the `data` objects of the MES endpoints. A payload that does not parse throws <see cref="JsonException"/>, so a bad
/// answer never replaces a good local cache.
/// </summary>
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "The DTOs are created by the JSON deserializer.")]
public static class MesBoardParser
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    /// <summary>
    /// Parses the operator board.
    /// </summary>
    /// <param name="json">The `data` object of the board answer.</param>
    /// <returns>The snapshot.</returns>
    /// <exception cref="JsonException">The payload is not a board.</exception>
    public static MesBoardSnapshot ParseBoard(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var board = Read<BoardDto>(json);
        var center = board.WorkCenter ?? throw new JsonException("The board has no work centre.");
        var rows = (board.Phases ?? []).Concat(board.Available ?? []).Select(ToRow).ToList();

        return new MesBoardSnapshot(board.ServerTime.UtcDateTime, center.Id, center.Name ?? string.Empty, rows);
    }

    /// <summary>
    /// Parses a single phase row, as the commands answer it.
    /// </summary>
    /// <param name="json">The `data` object of a command answer.</param>
    /// <returns>The row.</returns>
    public static MesPhaseRow ParseRow(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return ToRow(Read<RowDto>(json));
    }

    /// <summary>
    /// Parses the list of work centres.
    /// </summary>
    /// <param name="json">The `data` array of the work centres answer.</param>
    /// <returns>The work centres.</returns>
    public static IReadOnlyList<MesWorkCenter> ParseWorkCenters(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return Read<List<WorkCenterDto>>(json)
            .ConvertAll(center => new MesWorkCenter(center.Id, center.WorkCenterCode ?? string.Empty, center.Name ?? string.Empty));
    }

    private static T Read<T>(string json)
        where T : class =>
        JsonSerializer.Deserialize<T>(json, Options) ?? throw new JsonException("The MES payload is empty.");

    private static MesPhaseRow ToRow(RowDto row)
    {
        var order = row.ProductionOrder ?? throw new JsonException("The row has no production order.");
        var operation = row.Operation ?? throw new JsonException("The row has no operation.");
        var times = row.Times ?? throw new JsonException("The row has no times.");
        var declared = row.Declared ?? new DeclaredDto(GoodQuantity: null, ScrapQuantity: null);

        return new MesPhaseRow(
            row.Id,
            row.Version,
            Enumerated<MesPhaseStatus>(row.Status),
            row.ResumeTo is null ? null : Enumerated<MesPhaseStatus>(row.ResumeTo),
            Enumerated<MesMachineMode>(row.MachineMode),
            order.OrderNumber ?? string.Empty,
            order.ItemCode ?? string.Empty,
            order.ItemName ?? string.Empty,
            operation.Id,
            operation.Sequence,
            operation.Name ?? string.Empty,
            Quantity(order.TargetQuantity),
            Quantity(declared.GoodQuantity),
            Quantity(declared.ScrapQuantity),
            new MesTimeSummary(times.SetupSeconds, times.AttendedRunSeconds, times.UnattendedRunSeconds, times.DowntimeSeconds),
            ToOpenInterval(row.OpenInterval),
            (row.Components ?? []).ConvertAll(ToComponent));
    }

    private static MesOpenInterval? ToOpenInterval(OpenIntervalDto? open)
    {
        if (open is null)
        {
            return null;
        }

        MesSuspensionReason? reason = open.SuspensionReason is null ? null : Enumerated<MesSuspensionReason>(open.SuspensionReason);

        return new MesOpenInterval(Enumerated<MesTimeCategory>(open.Category), open.StartedAt.UtcDateTime, open.MachineAttended, reason);
    }

    private static MesComponent ToComponent(ComponentDto component) =>
        new(
            component.MfgItemId,
            component.ItemCode ?? string.Empty,
            component.ItemName ?? string.Empty,
            Quantity(component.RequiredQuantity),
            Quantity(component.ConsumedQuantity),
            component.LotTracked);

    private static TEnum Enumerated<TEnum>(string? wire)
        where TEnum : struct, Enum =>
        MesWire.TryParse(wire, out TEnum value) ? value : throw new JsonException($"Unknown {typeof(TEnum).Name} '{wire}'.");

    private static decimal Quantity(string? text) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : 0m;

    private sealed record BoardDto(DateTimeOffset ServerTime, WorkCenterDto? WorkCenter, List<RowDto>? Phases, List<RowDto>? Available);

    private sealed record WorkCenterDto(Guid Id, string? WorkCenterCode, string? Name);

    private sealed record RowDto(
        Guid? Id,
        int Version,
        string? Status,
        string? ResumeTo,
        string? MachineMode,
        OrderDto? ProductionOrder,
        OperationDto? Operation,
        OpenIntervalDto? OpenInterval,
        TimesDto? Times,
        DeclaredDto? Declared,
        List<ComponentDto>? Components);

    private sealed record OrderDto(string? OrderNumber, string? ItemCode, string? ItemName, string? TargetQuantity);

    private sealed record OperationDto(Guid Id, int Sequence, string? Name);

    private sealed record OpenIntervalDto(string? Category, DateTimeOffset StartedAt, bool MachineAttended, string? SuspensionReason);

    private sealed record TimesDto(long SetupSeconds, long AttendedRunSeconds, long UnattendedRunSeconds, long DowntimeSeconds);

    private sealed record DeclaredDto(string? GoodQuantity, string? ScrapQuantity);

    private sealed record ComponentDto(Guid MfgItemId, string? ItemCode, string? ItemName, string? RequiredQuantity, string? ConsumedQuantity, bool LotTracked);
}
