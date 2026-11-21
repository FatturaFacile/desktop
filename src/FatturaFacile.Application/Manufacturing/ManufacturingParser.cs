// <copyright file="ManufacturingParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Domain.Entities;
using static FatturaFacile.Application.Manufacturing.ManufacturingJson;

namespace FatturaFacile.Application.Manufacturing;

/// <summary>
/// Maps the `data` payloads of the web-api manufacturing endpoints (`work-centers`, `machines`, `working-calendars`, `routings`,
/// `oee/summary`) to the local entities. A payload that does not parse throws <see cref="JsonException"/>, so it never replaces a
/// good cache.
/// </summary>
public static class ManufacturingParser
{
    private static readonly string[] LossCategories =
    [
        "equipment_failure",
        "setup_adjustment",
        "minor_stops",
        "reduced_speed",
        "process_defects",
        "startup_rejects",
    ];

    /// <summary>
    /// Parses a list of work centres.
    /// </summary>
    /// <param name="json">The `data` array.</param>
    /// <param name="syncedAtUtc">When the data was pulled.</param>
    /// <returns>The work centres.</returns>
    public static IReadOnlyList<MfgWorkCenter> ParseWorkCenters(string json, DateTime syncedAtUtc) =>
        Elements(json).ConvertAll(e => new MfgWorkCenter
        {
            Id = RequiredGuid(e, "id"),
            WorkCenterCode = Str(e, "work_center_code"),
            Name = Str(e, "name"),
            Description = NullableStr(e, "description"),
            DepartmentCode = NullableStr(e, "department_code"),
            CapacityHoursPerDay = Dec(e, "capacity_hours_per_day"),
            EfficiencyRate = Dec(e, "efficiency_rate", 100m),
            AvailabilityRate = Dec(e, "availability_rate", 100m),
            HourlyCostRate = Dec(e, "hourly_cost_rate"),
            HourlyOverheadRate = Dec(e, "hourly_overhead_rate"),
            HourlySetupCostRate = Dec(e, "hourly_setup_cost_rate"),
            HourlyMachineRunCostRate = Dec(e, "hourly_machine_run_cost_rate"),
            CurrencyCode = NullableStr(e, "currency_code") ?? "CHF",
            Status = NullableStr(e, "status") ?? "active",
            WorkingCalendarId = NullableGuid(e, "working_calendar_id"),
            ParentWorkCenterId = NullableGuid(e, "parent_work_center_id"),
            CostCenterId = NullableGuid(e, "cost_center_id"),
            WarehouseId = NullableGuid(e, "warehouse_id"),
            ParallelCapacityUnits = Int(e, "parallel_capacity_units", 1),
            OeeTargetPercent = Dec(e, "oee_target_percent", 85m),
            AvailabilityTargetPercent = NullableDec(e, "availability_target_percent"),
            PerformanceTargetPercent = NullableDec(e, "performance_target_percent"),
            QualityTargetPercent = NullableDec(e, "quality_target_percent"),
            DefaultQueueWaitMinutes = Dec(e, "default_queue_wait_minutes"),
            DefaultMoveTimeMinutes = Dec(e, "default_move_time_minutes"),
            AlternateWorkCenterIdsJson = RawArray(e, "alternate_work_center_ids"),
            WeeklyHoursJson = RawArray(e, "calendar"),
            SyncedAtUtc = syncedAtUtc,
        });

    /// <summary>
    /// Parses a list of machines.
    /// </summary>
    /// <param name="json">The `data` array.</param>
    /// <param name="syncedAtUtc">When the data was pulled.</param>
    /// <returns>The machines.</returns>
    public static IReadOnlyList<MfgMachineCenter> ParseMachines(string json, DateTime syncedAtUtc) =>
        Elements(json).ConvertAll(e => new MfgMachineCenter
        {
            Id = RequiredGuid(e, "id"),
            MachineCode = Str(e, "machine_code"),
            Name = Str(e, "name"),
            WorkCenterId = RequiredGuid(e, "work_center_id"),
            FixedAssetId = NullableGuid(e, "fixed_asset_id"),
            Manufacturer = NullableStr(e, "manufacturer"),
            Model = NullableStr(e, "model"),
            SerialNumber = NullableStr(e, "serial_number"),
            IsIndustria40Enabled = Bool(e, "is_industria_40_enabled"),
            IsTransizione50Enabled = Bool(e, "is_transizione_50_enabled"),
            TelemetryStatus = NullableStr(e, "telemetry_status") ?? "never",
            TelemetryLastSeenAt = NullableDate(e, "telemetry_last_seen_at"),
            IdealCycleTimeSecondsPerUnit = Dec(e, "ideal_cycle_time_seconds_per_unit"),
            ParallelCapacityUnits = Int(e, "parallel_capacity_units", 1),
            EfficiencyFactorPercent = NullableDec(e, "efficiency_factor_percent"),
            HourlyMachineRunCostRateOverride = NullableDec(e, "hourly_machine_run_cost_rate_override"),
            Status = NullableStr(e, "status") ?? "active",
            SyncedAtUtc = syncedAtUtc,
        });

    /// <summary>
    /// Parses a list of working calendars with their shifts and closures.
    /// </summary>
    /// <param name="json">The `data` array.</param>
    /// <param name="syncedAtUtc">When the data was pulled.</param>
    /// <returns>The calendars.</returns>
    public static IReadOnlyList<MfgWorkingCalendar> ParseCalendars(string json, DateTime syncedAtUtc) =>
        Elements(json).ConvertAll(e =>
        {
            var calendar = new MfgWorkingCalendar
            {
                Id = RequiredGuid(e, "id"),
                Code = Str(e, "code"),
                Name = Str(e, "name"),
                Timezone = NullableStr(e, "timezone") ?? "UTC",
                CountryCode = NullableStr(e, "country_code"),
                RegionCode = NullableStr(e, "region_code"),
                InheritHolidayCalendarId = NullableGuid(e, "inherit_holiday_calendar_id"),
                IsDefault = Bool(e, "is_default"),
                SyncedAtUtc = syncedAtUtc,
            };

            foreach (var s in Items(e, "shifts"))
            {
                calendar.Shifts.Add(new MfgCalendarShift
                {
                    CalendarId = calendar.Id,
                    DayOfWeek = Int(s, "day_of_week"),
                    ShiftCode = Str(s, "shift_code"),
                    StartTime = Str(s, "start_time"),
                    EndTime = Str(s, "end_time"),
                    CrossesMidnight = Bool(s, "crosses_midnight"),
                    BreakMinutes = Int(s, "break_minutes"),
                    NetAvailableMinutes = Int(s, "net_available_minutes"),
                    LaborMultiplier = Dec(s, "labor_multiplier", 1m),
                });
            }

            foreach (var c in Items(e, "closures"))
            {
                calendar.Closures.Add(new MfgCalendarClosure
                {
                    CalendarId = calendar.Id,
                    StartsOn = RequiredDate(c, "starts_on"),
                    EndsOn = RequiredDate(c, "ends_on"),
                    Reason = NullableStr(c, "reason"),
                });
            }

            return calendar;
        });

    /// <summary>
    /// Parses a list of routings with their operation steps.
    /// </summary>
    /// <param name="json">The `data` array.</param>
    /// <param name="syncedAtUtc">When the data was pulled.</param>
    /// <returns>The routings.</returns>
    public static IReadOnlyList<MfgRouting> ParseRoutings(string json, DateTime syncedAtUtc) =>
        Elements(json).ConvertAll(e =>
        {
            var routing = new MfgRouting
            {
                Id = RequiredGuid(e, "id"),
                RoutingNumber = Str(e, "routing_number"),
                ItemId = RequiredGuid(e, "mfg_item_id"),
                ItemCode = NullableStr(e, "item_code"),
                ItemName = NullableStr(e, "item_name"),
                Name = Str(e, "name"),
                Revision = Str(e, "revision"),
                Status = Str(e, "status"),
                IsDefault = Bool(e, "is_default"),
                EffectiveFrom = NullableDate(e, "effective_from"),
                EffectiveTo = NullableDate(e, "effective_to"),
                Notes = NullableStr(e, "notes"),
                BomId = NullableGuid(e, "bom_id"),
                SyncedAtUtc = syncedAtUtc,
            };

            foreach (var o in Items(e, "operations"))
            {
                routing.Operations.Add(ParseOperation(o, routing.Id));
            }

            return routing;
        });

    /// <summary>
    /// Parses the answer of `oee/summary` into a snapshot.
    /// </summary>
    /// <param name="json">The `data` object.</param>
    /// <param name="syncedAtUtc">When the data was pulled.</param>
    /// <returns>The snapshot.</returns>
    /// <exception cref="JsonException">The payload is not an OEE summary.</exception>
    public static MfgOeeSnapshot ParseOeeSnapshot(string json, DateTime syncedAtUtc)
    {
        using var document = JsonDocument.Parse(json);
        var e = document.RootElement;
        var summary = Find(e, "summary") ?? throw new JsonException("The OEE answer has no summary.");
        var targets = Find(e, "targets_percent") ?? default;

        return new MfgOeeSnapshot
        {
            WorkCenterId = RequiredGuid(e, "work_center_id"),
            MachineCenterId = NullableGuid(e, "machine_center_id"),
            FromUtc = RequiredDate(e, "from"),
            ToUtc = RequiredDate(e, "to"),
            ScheduledMinutes = Dbl(summary, "scheduled_minutes"),
            PlannedProductionMinutes = Dbl(summary, "planned_production_minutes"),
            UnplannedDowntimeMinutes = Dbl(summary, "unplanned_downtime_minutes"),
            OperatingMinutes = Dbl(summary, "operating_minutes"),
            GoodUnits = Dbl(summary, "good_units"),
            ScrapUnits = Dbl(summary, "scrap_units"),
            ReworkUnits = Dbl(summary, "rework_units"),
            AvailabilityPercent = Dbl(summary, "availability_percent"),
            PerformancePercent = Dbl(summary, "performance_percent"),
            QualityPercent = Dbl(summary, "quality_percent"),
            OeePercent = Dbl(summary, "oee_percent"),
            OeeTargetPercent = Dbl(targets, "oee"),
            GapToTargetPercent = Dbl(e, "gap_to_target_percent"),
            MeetsTarget = Bool(e, "meets_target"),
            SixBigLossesJson = RawObject(e, "six_big_losses"),
            MachinesJson = RawArray(e, "machines"),
            SyncedAtUtc = syncedAtUtc,
        };
    }

    /// <summary>
    /// Parses the six big losses payload of a cached OEE snapshot.
    /// </summary>
    /// <param name="json">The `six_big_losses` object.</param>
    /// <returns>The losses in the order of the payload (empty when it is not an object).</returns>
    public static IReadOnlyList<OeeLossLine> ParseLosses(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = JsonDocument.Parse(json);
        List<OeeLossLine> losses = [];
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            return losses;
        }

        foreach (var category in LossCategories)
        {
            if (Find(document.RootElement, category) is { } loss)
            {
                losses.Add(new OeeLossLine(category, Dbl(loss, "minutes"), Str(loss, "factor")));
            }
        }

        return losses;
    }

    private static MfgRoutingOperation ParseOperation(JsonElement o, Guid routingId)
    {
        var operation = new MfgRoutingOperation
        {
            Id = RequiredGuid(o, "id"),
            RoutingId = routingId,
            Sequence = Int(o, "sequence"),
            OperationCode = Str(o, "operation_code"),
            Name = Str(o, "name"),
            WorkCenterId = RequiredGuid(o, "work_center_id"),
            MachineCenterId = NullableGuid(o, "machine_center_id"),
            SetupTimeMinutes = Dec(o, "setup_time_minutes"),
            RunTimeMinutesPerUnit = Dec(o, "run_time_minutes_per_unit"),
            LaborTimeMinutesPerUnit = Dec(o, "labor_time_minutes_per_unit"),
            IsSubcontracted = Bool(o, "is_subcontracted"),
            SubcontractorContactId = NullableGuid(o, "subcontractor_contact_id"),
            SubcontractUnitCost = Dec(o, "subcontract_unit_cost"),
            QualityPlanId = NullableGuid(o, "quality_plan_id"),
            OperationType = NullableStr(o, "operation_type") ?? "internal_machining",
            QueueTimeMinutes = Dec(o, "queue_time_minutes"),
            PostOperationWaitMinutes = Dec(o, "post_operation_wait_minutes"),
            MoveTimeMinutes = Dec(o, "move_time_minutes"),
            OverlapTransferBatchQty = NullableDec(o, "overlap_transfer_batch_qty"),
            RequiredOperators = Int(o, "required_operators", 1),
            AlternateWorkCenterIdsJson = RawArray(o, "alternate_work_center_ids"),
            LinkedQualityControlPointId = NullableGuid(o, "linked_quality_control_point_id"),
            WorkInstructionDocId = NullableGuid(o, "work_instruction_doc_id"),
        };

        foreach (var link in Items(o, "bom_components"))
        {
            operation.BomComponents.Add(new MfgRoutingOperationBomLink
            {
                OperationId = operation.Id,
                BomComponentId = RequiredGuid(link, "bom_component_id"),
                BackflushOnCompletion = Bool(link, "backflush_on_completion"),
                Notes = NullableStr(link, "notes"),
            });
        }

        return operation;
    }

    private static List<JsonElement> Elements(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("The manufacturing payload is not a list.");
        }

        List<JsonElement> items = [];
        for (var i = 0; i < root.GetArrayLength(); i++)
        {
            items.Add(root[i].Clone());
        }

        return items;
    }
}
