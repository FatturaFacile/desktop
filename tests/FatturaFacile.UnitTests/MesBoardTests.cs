// <copyright file="MesBoardTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Application.Mes;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the board parser and of the projection of the pending commands onto the last board read.
/// </summary>
public sealed class MesBoardTests
{
    private const string BoardJson = """{"server_time":"2026-06-30T08:00:00+00:00","work_center":{"id":"11111111-1111-1111-1111-111111111111","work_center_code":"WC-01","name":"Milling"},"phases":[{"id":"22222222-2222-2222-2222-222222222222","version":4,"status":"running","resume_to":null,"machine_mode":"attended","production_order":{"order_number":"PO-100","item_code":"ITM-1","item_name":"Bracket","target_quantity":"100.0000"},"operation":{"id":"33333333-3333-3333-3333-333333333333","sequence":10,"name":"Milling"},"open_interval":{"category":"run","started_at":"2026-06-30T07:30:00+00:00","machine_attended":true,"suspension_reason":null},"times":{"setup_seconds":600,"attended_run_seconds":1800,"unattended_run_seconds":0,"downtime_seconds":120},"declared":{"good_quantity":"40.0000","scrap_quantity":"2.0000"},"components":[{"mfg_item_id":"44444444-4444-4444-4444-444444444444","item_code":"STEEL","item_name":"Steel bar","required_quantity":"10.0000","consumed_quantity":"4.0000","lot_tracked":true}]}],"available":[{"id":null,"version":0,"status":"available","machine_mode":"attended","production_order":{"order_number":"PO-101","item_code":"ITM-2","item_name":"Plate","target_quantity":"5"},"operation":{"id":"55555555-5555-5555-5555-555555555555","sequence":20,"name":"Drilling"},"times":{"setup_seconds":0,"attended_run_seconds":0,"unattended_run_seconds":0,"downtime_seconds":0}}]}""";

    private static readonly DateTime ServerTime = new(2026, 6, 30, 8, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// The board parses the work centre, the active phases and the available operations with their counters and components.
    /// </summary>
    [Fact]
    public void ParseBoard_ValidPayload_MapsEveryField()
    {
        var snapshot = MesBoardParser.ParseBoard(BoardJson);

        Assert.Equal(ServerTime, snapshot.ServerTimeUtc);
        Assert.Equal("Milling", snapshot.WorkCenterName);
        Assert.Equal(2, snapshot.Rows.Count);

        var running = snapshot.Rows[0];
        Assert.Equal(MesPhaseStatus.Running, running.Status);
        Assert.Equal(4, running.Version);
        Assert.Equal(40m, running.GoodQuantity);
        Assert.Equal(2m, running.ScrapQuantity);
        Assert.Equal(100m, running.TargetQuantity);
        Assert.Equal(2400, running.Times.ManSeconds);
        Assert.Equal(MesTimeCategory.Run, running.OpenInterval?.Category);
        Assert.Equal(new DateTime(2026, 6, 30, 7, 30, 0, DateTimeKind.Utc), running.OpenInterval?.StartedAtUtc);
        var component = Assert.Single(running.Components);
        Assert.True(component.LotTracked);
        Assert.Equal(10m, component.RequiredQuantity);

        var available = snapshot.Rows[1];
        Assert.Null(available.PhaseId);
        Assert.Equal(MesPhaseStatus.Available, available.Status);
        Assert.Equal("op:55555555-5555-5555-5555-555555555555", available.Key);
        Assert.Empty(available.Components);
    }

    /// <summary>
    /// A payload with an unknown state is rejected, so a bad answer never replaces a good cache.
    /// </summary>
    [Fact]
    public void ParseBoard_UnknownStatus_ThrowsJsonException()
    {
        var bad = BoardJson.Replace("\"status\":\"running\"", "\"status\":\"exploded\"", StringComparison.Ordinal);

        _ = Assert.Throws<JsonException>(() => MesBoardParser.ParseBoard(bad));
        _ = Assert.Throws<JsonException>(() => MesBoardParser.ParseBoard("{}"));
    }

    /// <summary>
    /// The work centres list parses code and name.
    /// </summary>
    [Fact]
    public void ParseWorkCenters_List_MapsCodeAndName()
    {
        var centers = MesBoardParser.ParseWorkCenters("""[{"id":"11111111-1111-1111-1111-111111111111","work_center_code":"WC-01","name":"Milling"}]""");

        var center = Assert.Single(centers);
        Assert.Equal("WC-01 · Milling", center.Display);
    }

    /// <summary>
    /// Pending commands move the phase through the state machine from the instants the operator acted: the counters of the
    /// running, suspended and resumed intervals add up to the elapsed time.
    /// </summary>
    [Fact]
    public void Project_SuspendAndResume_AccumulatesRunAndDowntime()
    {
        var operationId = Guid.NewGuid();
        var run = new MesOpenInterval(MesTimeCategory.Run, ServerTime, MachineAttended: true, Reason: null);
        var row = MesTestRows.Row(MesPhaseStatus.Running, Guid.NewGuid(), operationId, run);
        var suspend = MesCommand.Suspend(row, MesSuspensionReason.ToolChange, ServerTime.AddSeconds(60));
        var resume = MesCommand.Resume(row, ServerTime.AddSeconds(120));

        var projected = MesLocalProjection.Project([row], ServerTime, [resume, suspend], ServerTime.AddSeconds(180)).Single();

        Assert.Equal(MesPhaseStatus.Running, projected.Status);
        Assert.Equal(120, projected.Times.AttendedRunSeconds);
        Assert.Equal(60, projected.Times.DowntimeSeconds);
        Assert.Equal(MesTimeCategory.Run, projected.OpenInterval?.Category);
    }

    /// <summary>
    /// While suspended the open interval is the suspension with its reason, and the declared pieces are added to the counters.
    /// </summary>
    [Fact]
    public void Project_SuspendThenDeclare_ShowsReasonAndPieces()
    {
        var run = new MesOpenInterval(MesTimeCategory.Run, ServerTime, MachineAttended: true, Reason: null);
        var row = MesTestRows.Row(MesPhaseStatus.Running, Guid.NewGuid(), open: run);
        var suspend = MesCommand.Suspend(row, MesSuspensionReason.MaterialShortage, ServerTime.AddSeconds(30));
        var declare = MesCommand.Declare(row, 8m, 1m, MesDefectReason.ToolWear, ServerTime.AddSeconds(40));

        var projected = MesLocalProjection.Project([row], ServerTime, [suspend, declare], ServerTime.AddSeconds(100)).Single();

        Assert.Equal(MesPhaseStatus.Suspended, projected.Status);
        Assert.Equal(MesSuspensionReason.MaterialShortage, projected.OpenInterval?.Reason);
        Assert.Equal(MesPhaseStatus.Running, projected.ResumeTo);
        Assert.Equal(8m, projected.GoodQuantity);
        Assert.Equal(1m, projected.ScrapQuantity);
        Assert.Equal(30, projected.Times.AttendedRunSeconds);
        Assert.Equal(70, projected.Times.DowntimeSeconds);
    }

    /// <summary>
    /// A command the state machine does not allow is not projected: the server will refuse it, so the board must not show its effect.
    /// </summary>
    [Fact]
    public void Project_DisallowedCommand_IsIgnored()
    {
        var run = new MesOpenInterval(MesTimeCategory.Run, ServerTime, MachineAttended: true, Reason: null);
        var row = MesTestRows.Row(MesPhaseStatus.Running, Guid.NewGuid(), open: run);
        var setup = MesCommand.StartSetup(row, ServerTime.AddSeconds(10));

        var projected = MesLocalProjection.Project([row], ServerTime, [setup], ServerTime.AddSeconds(20)).Single();

        Assert.Equal(MesPhaseStatus.Running, projected.Status);
        Assert.Equal(20, projected.Times.AttendedRunSeconds);
    }

    /// <summary>
    /// A consumed lot is added to the consumed quantity of its component.
    /// </summary>
    [Fact]
    public void Project_ConsumeMaterial_AddsToTheComponent()
    {
        var itemId = Guid.NewGuid();
        MesComponent[] components = [new(itemId, "STEEL", "Steel bar", 10m, 4m, LotTracked: true)];
        var run = new MesOpenInterval(MesTimeCategory.Run, ServerTime, MachineAttended: true, Reason: null);
        var row = MesTestRows.Row(MesPhaseStatus.Running, Guid.NewGuid(), open: run, components: components);
        var consume = MesCommand.ConsumeMaterial(row, itemId, "LOT-1", 2.5m, overConsumptionConfirmed: false, ServerTime.AddSeconds(5));

        var projected = MesLocalProjection.Project([row], ServerTime, [consume], ServerTime.AddSeconds(5)).Single();

        Assert.Equal(6.5m, projected.Components.Single().ConsumedQuantity);
    }

    /// <summary>
    /// Rows without pending commands stay as read, unless the board comes from the cache and must catch up to now.
    /// </summary>
    [Fact]
    public void Project_UntouchedRow_AdvancesOnlyWhenAskedTo()
    {
        var run = new MesOpenInterval(MesTimeCategory.Run, ServerTime, MachineAttended: true, Reason: null);
        var row = MesTestRows.Row(MesPhaseStatus.Running, Guid.NewGuid(), open: run);
        var now = ServerTime.AddSeconds(45);

        var asRead = MesLocalProjection.Project([row], ServerTime, [], now).Single();
        var caughtUp = MesLocalProjection.Project([row], ServerTime, [], now, advanceUntouched: true).Single();

        Assert.Equal(0, asRead.Times.AttendedRunSeconds);
        Assert.Equal(45, caughtUp.Times.AttendedRunSeconds);
    }

    /// <summary>
    /// A take-over of an available operation projects it as assigned, before any answer from the server.
    /// </summary>
    [Fact]
    public void Project_TakeOver_AssignsTheAvailableOperation()
    {
        var operationId = Guid.NewGuid();
        var row = MesTestRows.Row(MesPhaseStatus.Available, operationId: operationId);
        var take = MesCommand.TakeOver(operationId, phaseId: null, Guid.NewGuid(), ServerTime.AddSeconds(1));

        var projected = MesLocalProjection.Project([row], ServerTime, [take], ServerTime.AddSeconds(2)).Single();

        Assert.Equal(MesPhaseStatus.Assigned, projected.Status);
        Assert.Null(projected.OpenInterval);
    }
}
