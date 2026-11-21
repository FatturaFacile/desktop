// <copyright file="ManufacturingParserTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.
using System.Text.Json;
using FatturaFacile.Application.Manufacturing;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the mapping of the web-api manufacturing payloads to the local entities.
/// </summary>
public sealed class ManufacturingParserTests
{
    private static readonly DateTime At = new(2026, 11, 21, 9, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Work centres read decimals sent as strings or numbers, defaults for missing fields and the alternate centres.
    /// </summary>
    [Fact]
    public void ParseWorkCenters_MapsFieldsAndDefaults()
    {
        var centers = ManufacturingParser.ParseWorkCenters(ManufacturingTestData.WorkCenters, At);

        Assert.Equal(2, centers.Count);
        var milling = centers[0];
        Assert.Equal(ManufacturingTestData.WorkCenterA, milling.Id);
        Assert.Equal("WC-01", milling.WorkCenterCode);
        Assert.Equal("MECH", milling.DepartmentCode);
        Assert.Equal(8m, milling.CapacityHoursPerDay);
        Assert.Equal(90m, milling.EfficiencyRate);
        Assert.Equal(120m, milling.HourlyMachineRunCostRate);
        Assert.Equal(2, milling.ParallelCapacityUnits);
        Assert.Equal(ManufacturingTestData.Calendar, milling.WorkingCalendarId);
        Assert.Null(milling.ParentWorkCenterId);
        Assert.Null(milling.AvailabilityTargetPercent);
        Assert.Equal(10m, milling.DefaultQueueWaitMinutes);
        Assert.Contains("22222222-2222-2222-2222-222222222222", milling.AlternateWorkCenterIdsJson, StringComparison.Ordinal);
        Assert.Contains("available_hours", milling.WeeklyHoursJson, StringComparison.Ordinal);
        Assert.Equal(At, milling.SyncedAtUtc);

        var assembly = centers[1];
        Assert.Equal("maintenance", assembly.Status);
        Assert.Equal(100m, assembly.EfficiencyRate);
        Assert.Equal(85m, assembly.OeeTargetPercent);
        Assert.Equal(1, assembly.ParallelCapacityUnits);
        Assert.Equal("[]", assembly.AlternateWorkCenterIdsJson);
    }

    /// <summary>
    /// Machines read the work centre link, the Industria 4.0 flags, the telemetry and the overrides.
    /// </summary>
    [Fact]
    public void ParseMachines_MapsFlagsTelemetryAndOverrides()
    {
        var machine = Assert.Single(ManufacturingParser.ParseMachines(ManufacturingTestData.Machines, At));

        Assert.Equal(ManufacturingTestData.WorkCenterA, machine.WorkCenterId);
        Assert.True(machine.IsIndustria40Enabled);
        Assert.False(machine.IsTransizione50Enabled);
        Assert.Equal(new DateTime(2026, 11, 21, 8, 0, 0, DateTimeKind.Utc), machine.TelemetryLastSeenAt);
        Assert.Equal(30m, machine.IdealCycleTimeSecondsPerUnit);
        Assert.Equal(80m, machine.EfficiencyFactorPercent);
        Assert.Null(machine.HourlyMachineRunCostRateOverride);
    }

    /// <summary>
    /// Calendars come with their shifts and closures linked to the calendar.
    /// </summary>
    [Fact]
    public void ParseCalendars_MapsShiftsAndClosures()
    {
        var calendar = Assert.Single(ManufacturingParser.ParseCalendars(ManufacturingTestData.Calendars, At));

        Assert.Equal("Europe/Zurich", calendar.Timezone);
        Assert.True(calendar.IsDefault);
        Assert.Equal(2, calendar.Shifts.Count);
        Assert.All(calendar.Shifts, shift => Assert.Equal(calendar.Id, shift.CalendarId));
        var late = calendar.Shifts.Last();
        Assert.Equal("B", late.ShiftCode);
        Assert.Equal("14:00", late.StartTime);
        Assert.Equal(450, late.NetAvailableMinutes);
        Assert.Equal(1.1m, late.LaborMultiplier);
        var closure = Assert.Single(calendar.Closures);
        Assert.Equal(new DateTime(2026, 12, 24, 0, 0, 0, DateTimeKind.Utc), closure.StartsOn);
        Assert.Equal("Christmas", closure.Reason);
    }

    /// <summary>
    /// Routings carry their operation steps with the linked BOM components, quality point and work instruction.
    /// </summary>
    [Fact]
    public void ParseRoutings_MapsOperationsAndLinks()
    {
        var routing = Assert.Single(ManufacturingParser.ParseRoutings(ManufacturingTestData.Routings, At));

        Assert.Equal("RT-100", routing.RoutingNumber);
        Assert.Equal("B", routing.Revision);
        Assert.Equal("FG-1", routing.ItemCode);
        Assert.True(routing.IsDefault);
        Assert.Equal(2, routing.Operations.Count);

        var mill = routing.Operations.Single(o => o.Sequence == 10);
        Assert.Equal(routing.Id, mill.RoutingId);
        Assert.Equal(ManufacturingTestData.Machine, mill.MachineCenterId);
        Assert.Equal(30m, mill.SetupTimeMinutes);
        Assert.Equal(5m, mill.OverlapTransferBatchQty);
        Assert.Equal(ManufacturingTestData.QualityPoint, mill.LinkedQualityControlPointId);
        Assert.Equal(ManufacturingTestData.WorkInstruction, mill.WorkInstructionDocId);
        var link = Assert.Single(mill.BomComponents);
        Assert.Equal(ManufacturingTestData.BomComponent, link.BomComponentId);
        Assert.Equal(mill.Id, link.OperationId);
        Assert.True(link.BackflushOnCompletion);

        var assembly = routing.Operations.Single(o => o.Sequence == 20);
        Assert.Equal(2, assembly.RequiredOperators);
        Assert.Equal("manual_assembly", assembly.OperationType);
        Assert.Empty(assembly.BomComponents);
        Assert.Null(assembly.OverlapTransferBatchQty);
    }

    /// <summary>
    /// The OEE summary maps the figures, the target and keeps the losses and the machines payloads.
    /// </summary>
    [Fact]
    public void ParseOeeSnapshot_MapsTheSummary()
    {
        var snapshot = ManufacturingParser.ParseOeeSnapshot(ManufacturingTestData.OeeSummary, At);

        Assert.Equal(ManufacturingTestData.WorkCenterA, snapshot.WorkCenterId);
        Assert.Null(snapshot.MachineCenterId);
        Assert.Equal(new DateTime(2026, 11, 21, 8, 0, 0, DateTimeKind.Utc), snapshot.ToUtc);
        Assert.Equal(66.67d, snapshot.OeePercent);
        Assert.Equal(85d, snapshot.OeeTargetPercent);
        Assert.Equal(-18.33d, snapshot.GapToTargetPercent);
        Assert.False(snapshot.MeetsTarget);
        Assert.Equal(300d, snapshot.GoodUnits);
        Assert.Contains("equipment_failure", snapshot.SixBigLossesJson, StringComparison.Ordinal);
        Assert.Contains("M-01", snapshot.MachinesJson, StringComparison.Ordinal);
    }

    /// <summary>
    /// The six big losses payload becomes one line per known category.
    /// </summary>
    [Fact]
    public void ParseLosses_ReadsTheSixCategories()
    {
        var snapshot = ManufacturingParser.ParseOeeSnapshot(ManufacturingTestData.OeeSummary, At);

        var losses = ManufacturingParser.ParseLosses(snapshot.SixBigLossesJson);

        Assert.Equal(6, losses.Count);
        var failure = losses.Single(l => string.Equals(l.Category, "equipment_failure", StringComparison.Ordinal));
        Assert.Equal(60d, failure.Minutes);
        Assert.Equal("availability", failure.Factor);
        Assert.Empty(ManufacturingParser.ParseLosses("[]"));
    }

    /// <summary>
    /// A payload that is not what it should be raises a JSON exception, so it never replaces a good cache.
    /// </summary>
    [Fact]
    public void Parse_MalformedPayloads_Throw()
    {
        _ = Assert.ThrowsAny<JsonException>(() => ManufacturingParser.ParseWorkCenters("{}", At));
        _ = Assert.ThrowsAny<JsonException>(() => ManufacturingParser.ParseWorkCenters("""[{ "name": "no id" }]""", At));
        _ = Assert.ThrowsAny<JsonException>(() => ManufacturingParser.ParseWorkCenters("not json", At));
        _ = Assert.ThrowsAny<JsonException>(() => ManufacturingParser.ParseOeeSnapshot("{}", At));
    }
}
