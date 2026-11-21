// <copyright file="ManufacturingTestData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable SA1600, MA0136, MA0176, MA0075 // Test data: undocumented shared samples, positional literals and plain concatenation.
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Web-api shaped manufacturing payloads and entity builders shared by the Mod 061 tests.
/// </summary>
internal static class ManufacturingTestData
{
    public static readonly Guid WorkCenterA = new("11111111-1111-1111-1111-111111111111");
    public static readonly Guid WorkCenterB = new("22222222-2222-2222-2222-222222222222");
    public static readonly Guid Machine = new("33333333-3333-3333-3333-333333333333");
    public static readonly Guid Calendar = new("44444444-4444-4444-4444-444444444444");
    public static readonly Guid Routing = new("55555555-5555-5555-5555-555555555555");
    public static readonly Guid OperationOne = new("66666666-6666-6666-6666-666666666601");
    public static readonly Guid OperationTwo = new("66666666-6666-6666-6666-666666666602");
    public static readonly Guid Item = new("77777777-7777-7777-7777-777777777777");
    public static readonly Guid BomComponent = new("88888888-8888-8888-8888-888888888888");
    public static readonly Guid QualityPoint = new("99999999-9999-9999-9999-999999999999");
    public static readonly Guid WorkInstruction = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public static readonly string WorkCenters = """
        [
          {
            "id": "11111111-1111-1111-1111-111111111111",
            "work_center_code": "WC-01",
            "name": "Milling",
            "description": "CNC milling",
            "department_code": "MECH",
            "capacity_hours_per_day": "8.00",
            "efficiency_rate": "90.00",
            "availability_rate": "95.00",
            "hourly_cost_rate": "40.0000",
            "hourly_overhead_rate": "15.0000",
            "hourly_setup_cost_rate": "50.0000",
            "hourly_machine_run_cost_rate": 120,
            "status": "active",
            "working_calendar_id": "44444444-4444-4444-4444-444444444444",
            "parent_work_center_id": null,
            "parallel_capacity_units": 2,
            "oee_target_percent": "85.00",
            "availability_target_percent": null,
            "currency_code": "CHF",
            "default_queue_wait_minutes": "10",
            "default_move_time_minutes": "5",
            "alternate_work_center_ids": ["22222222-2222-2222-2222-222222222222"],
            "calendar": [{ "weekday": 1, "available_hours": "8.00" }]
          },
          {
            "id": "22222222-2222-2222-2222-222222222222",
            "work_center_code": "WC-02",
            "name": "Assembly",
            "status": "maintenance"
          }
        ]
        """;

    public static readonly string Machines = """
        [
          {
            "id": "33333333-3333-3333-3333-333333333333",
            "machine_code": "M-01",
            "name": "Mill 1",
            "work_center_id": "11111111-1111-1111-1111-111111111111",
            "manufacturer": "Acme",
            "is_industria_40_enabled": true,
            "is_transizione_50_enabled": false,
            "telemetry_status": "online",
            "telemetry_last_seen_at": "2026-11-21T08:00:00+00:00",
            "ideal_cycle_time_seconds_per_unit": "30.00",
            "parallel_capacity_units": 1,
            "efficiency_factor_percent": "80.00",
            "hourly_machine_run_cost_rate_override": null,
            "status": "active"
          }
        ]
        """;

    public static readonly string Calendars = """
        [
          {
            "id": "44444444-4444-4444-4444-444444444444",
            "code": "STD",
            "name": "Standard",
            "timezone": "Europe/Zurich",
            "country_code": "CH",
            "is_default": true,
            "shifts": [
              { "day_of_week": 1, "shift_code": "A", "start_time": "06:00", "end_time": "14:00", "crosses_midnight": false, "break_minutes": 30, "net_available_minutes": 450, "labor_multiplier": "1.0000" },
              { "day_of_week": 1, "shift_code": "B", "start_time": "14:00", "end_time": "22:00", "crosses_midnight": false, "break_minutes": 30, "net_available_minutes": 450, "labor_multiplier": "1.1000" }
            ],
            "closures": [
              { "starts_on": "2026-12-24", "ends_on": "2026-12-26", "reason": "Christmas" }
            ]
          }
        ]
        """;

    public static readonly string Routings = """
        [
          {
            "id": "55555555-5555-5555-5555-555555555555",
            "routing_number": "RT-100",
            "mfg_item_id": "77777777-7777-7777-7777-777777777777",
            "item_code": "FG-1",
            "item_name": "Bracket",
            "name": "Bracket routing",
            "revision": "B",
            "status": "released",
            "is_default": true,
            "effective_from": "2026-01-01",
            "effective_to": null,
            "bom_id": null,
            "operations_count": 2,
            "operations": [
              {
                "id": "66666666-6666-6666-6666-666666666602",
                "sequence": 20,
                "operation_code": "ASM",
                "name": "Assemble",
                "work_center_id": "22222222-2222-2222-2222-222222222222",
                "machine_center_id": null,
                "setup_time_minutes": "0",
                "run_time_minutes_per_unit": "1.5",
                "labor_time_minutes_per_unit": "0",
                "is_subcontracted": false,
                "subcontract_unit_cost": "0",
                "operation_type": "manual_assembly",
                "queue_time_minutes": "0",
                "post_operation_wait_minutes": "0",
                "move_time_minutes": "0",
                "overlap_transfer_batch_qty": null,
                "required_operators": 2,
                "alternate_work_center_ids": [],
                "bom_components": []
              },
              {
                "id": "66666666-6666-6666-6666-666666666601",
                "sequence": 10,
                "operation_code": "MILL",
                "name": "Mill the blank",
                "work_center_id": "11111111-1111-1111-1111-111111111111",
                "machine_center_id": "33333333-3333-3333-3333-333333333333",
                "setup_time_minutes": "30",
                "run_time_minutes_per_unit": "2",
                "labor_time_minutes_per_unit": "2",
                "is_subcontracted": false,
                "subcontract_unit_cost": "0",
                "quality_plan_id": null,
                "operation_type": "internal_machining",
                "queue_time_minutes": "10",
                "post_operation_wait_minutes": "0",
                "move_time_minutes": "5",
                "overlap_transfer_batch_qty": "5",
                "required_operators": 1,
                "alternate_work_center_ids": [],
                "linked_quality_control_point_id": "99999999-9999-9999-9999-999999999999",
                "work_instruction_doc_id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                "bom_components": [
                  { "bom_component_id": "88888888-8888-8888-8888-888888888888", "backflush_on_completion": true, "notes": "steel" }
                ]
              }
            ]
          }
        ]
        """;

    public static readonly string OeeSummary = """
        {
          "work_center_id": "11111111-1111-1111-1111-111111111111",
          "machine_center_id": null,
          "from": "2026-10-22T08:00:00+00:00",
          "to": "2026-11-21T08:00:00+00:00",
          "granularity": "custom_range",
          "summary": {
            "scheduled_minutes": 480,
            "planned_production_minutes": 450,
            "unplanned_downtime_minutes": 60,
            "operating_minutes": 390,
            "good_units": 300,
            "scrap_units": 10,
            "rework_units": 10,
            "total_units": 320,
            "availability_percent": 86.67,
            "performance_percent": 82.05,
            "quality_percent": 93.75,
            "oee_percent": 66.67
          },
          "targets_percent": { "oee": 85, "availability": null, "performance": null, "quality": null },
          "meets_target": false,
          "gap_to_target_percent": -18.33,
          "six_big_losses": {
            "equipment_failure": { "minutes": 60, "factor": "availability" },
            "setup_adjustment": { "minutes": 0, "factor": "availability" },
            "minor_stops": { "minutes": 0, "factor": "performance" },
            "reduced_speed": { "minutes": 0, "factor": "performance" },
            "process_defects": { "minutes": 20, "factor": "quality" },
            "startup_rejects": { "minutes": 0, "factor": "quality" }
          },
          "machines": [
            { "machine_center_id": "33333333-3333-3333-3333-333333333333", "machine_code": "M-01", "telemetry_status": "online" }
          ]
        }
        """;

    public static MfgWorkCenter WorkCenter(Guid id, string code = "WC", decimal setupRate = 60m, decimal machineRate = 120m, decimal laborRate = 30m, decimal overheadRate = 12m) => new()
    {
        Id = id,
        WorkCenterCode = code,
        Name = code,
        HourlySetupCostRate = setupRate,
        HourlyMachineRunCostRate = machineRate,
        HourlyCostRate = laborRate,
        HourlyOverheadRate = overheadRate,
        EfficiencyRate = 100m,
        ParallelCapacityUnits = 1,
    };

    public static MfgRoutingOperation Operation(int sequence, Guid workCenter, decimal setup, decimal run, decimal queue = 0m, decimal move = 0m, decimal? overlap = null) => new()
    {
        Sequence = sequence,
        OperationCode = "OP" + sequence,
        Name = "Operation " + sequence,
        WorkCenterId = workCenter,
        SetupTimeMinutes = setup,
        RunTimeMinutesPerUnit = run,
        QueueTimeMinutes = queue,
        MoveTimeMinutes = move,
        OverlapTransferBatchQty = overlap,
    };

    public static MfgRouting RoutingOf(params MfgRoutingOperation[] operations)
    {
        var routing = new MfgRouting { RoutingNumber = "R", Revision = "A", Status = "released" };
        foreach (var operation in operations)
        {
            operation.RoutingId = routing.Id;
            routing.Operations.Add(operation);
        }

        return routing;
    }
}
