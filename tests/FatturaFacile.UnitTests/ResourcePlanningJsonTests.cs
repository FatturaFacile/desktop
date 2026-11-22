// <copyright file="ResourcePlanningJsonTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003, MA0002, MA0006 // Positional literals and plain string comparisons read better unnamed in tests.
using System.Text.Json;
using FatturaFacile.Application.ResourcePlanning;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the Mod 079 models and their JSON: snake case members, the booking type names, enum converters and null handling.
/// </summary>
public sealed class ResourcePlanningJsonTests
{
    private const string AllocationJson = """
        {
          "id": "88888888-8888-8888-8888-888888888888",
          "project_id": "33333333-3333-3333-3333-333333333333",
          "project_name": "Plant retrofit",
          "wbs_node_id": null,
          "wbs_node_title": null,
          "employee_id": "11111111-1111-1111-1111-111111111111",
          "employee_name": "Anna Rossi",
          "role_id": null,
          "booking_type": "SoftTentative",
          "start_date": "2026-11-16",
          "end_date": "2026-11-27",
          "allocated_hours_per_day": "6.00",
          "total_allocated_hours": 60,
          "is_billable": true,
          "crm_opportunity_id": null,
          "crm_opportunity_name": null,
          "is_overloaded": true,
          "peak_load_percent": 112.5,
          "notes": null
        }
        """;

    /// <summary>
    /// An allocation of the API is read with dates, decimals sent as strings and the booking type.
    /// </summary>
    [Fact]
    public void Read_Allocation_MapsEveryMember()
    {
        var allocation = ResourcePlanningJson.Read<ResourceAllocation>(AllocationJson);

        Assert.Equal(ResourcePlanningTestData.AllocationOne, allocation.Id);
        Assert.Equal(ResourcePlanningTestData.ProjectA, allocation.ProjectId);
        Assert.Null(allocation.WbsNodeId);
        Assert.Equal(ResourceBookingType.SoftTentative, allocation.BookingType);
        Assert.Equal(new DateOnly(2026, 11, 16), allocation.StartDate);
        Assert.Equal(new DateOnly(2026, 11, 27), allocation.EndDate);
        Assert.Equal(6m, allocation.AllocatedHoursPerDay);
        Assert.Equal(60m, allocation.TotalAllocatedHours);
        Assert.True(allocation.IsBillable);
        Assert.True(allocation.IsOverloaded);
        Assert.Equal(112.5d, allocation.PeakLoadPercent);
        Assert.Null(allocation.Notes);
    }

    /// <summary>
    /// The booking type is written with the exact names of the API.
    /// </summary>
    /// <param name="type">The booking type.</param>
    /// <param name="expected">The name on the wire.</param>
    [Theory]
    [InlineData(ResourceBookingType.SoftTentative, "SoftTentative")]
    [InlineData(ResourceBookingType.HardCommitted, "HardCommitted")]
    public void Write_BookingType_UsesTheNamesOfTheContract(ResourceBookingType type, string expected)
    {
        var body = ResourcePlanningJson.Write(new AllocationRequest(null, null, ResourcePlanningTestData.Anna, null, type, new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 20), 4m, true, null, null, true));

        using var document = JsonDocument.Parse(body);
        Assert.Equal(expected, document.RootElement.GetProperty("booking_type").GetString());
        Assert.Equal(expected, ResourceBookingTypeJsonConverter.ToWire(type));
    }

    /// <summary>
    /// An unknown booking type is rejected instead of being mapped to a default.
    /// </summary>
    [Fact]
    public void Read_UnknownBookingType_Throws()
    {
        var json = AllocationJson.Replace("SoftTentative", "Maybe", StringComparison.Ordinal);

        _ = Assert.Throws<JsonException>(() => ResourcePlanningJson.Read<ResourceAllocation>(json));
    }

    /// <summary>
    /// A request is written in snake case with dates as Y-m-d and the members that are null left out.
    /// </summary>
    [Fact]
    public void Write_AllocationRequest_IsSnakeCaseWithoutNulls()
    {
        var request = new AllocationRequest(ResourcePlanningTestData.ProjectA, null, ResourcePlanningTestData.Anna, null, ResourceBookingType.HardCommitted, new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 20), 4.5m, IsBillable: false, null, "night shift", AllowOverload: false);

        using var document = JsonDocument.Parse(ResourcePlanningJson.Write(request));
        var root = document.RootElement;

        Assert.Equal("2026-11-16", root.GetProperty("start_date").GetString());
        Assert.Equal("2026-11-20", root.GetProperty("end_date").GetString());
        Assert.Equal(4.5m, root.GetProperty("allocated_hours_per_day").GetDecimal());
        Assert.False(root.GetProperty("is_billable").GetBoolean());
        Assert.False(root.GetProperty("allow_overload").GetBoolean());
        Assert.Equal("night shift", root.GetProperty("notes").GetString());
        Assert.False(root.TryGetProperty("wbs_node_id", out _));
        Assert.False(root.TryGetProperty("role_id", out _));
        Assert.False(root.TryGetProperty("crm_opportunity_id", out _));
    }

    /// <summary>
    /// A rebalance is written with the strategy in lower case and only the parameters that were given.
    /// </summary>
    [Fact]
    public void Write_RebalanceRequest_UsesTheLowerCaseStrategy()
    {
        var request = new RebalanceRequest(RebalanceStrategy.Split, ResourcePlanningTestData.Bruno, null, null, new DateOnly(2026, 11, 23), null, null);

        using var document = JsonDocument.Parse(ResourcePlanningJson.Write(request));
        var root = document.RootElement;

        Assert.Equal("split", root.GetProperty("strategy").GetString());
        Assert.Equal("2026-11-23", root.GetProperty("split_date").GetString());
        Assert.Equal(ResourcePlanningTestData.Bruno, root.GetProperty("employee_id").GetGuid());
        Assert.False(root.TryGetProperty("shift_days", out _));
    }

    /// <summary>
    /// The heatmap is read with its thresholds, periods, rows, cells and the band names of the API.
    /// </summary>
    [Fact]
    public void Read_Heatmap_MapsThresholdsPeriodsAndCells()
    {
        const string json = """
            {
              "from": "2026-11-16", "to": "2026-11-29", "granularity": "week",
              "thresholds": { "yellow": 80, "red": 100 },
              "periods": ["2026-11-16", "2026-11-23"],
              "rows": [ {
                "employee_id": "11111111-1111-1111-1111-111111111111", "employee_name": "Anna Rossi",
                "department_id": null, "department_name": "Workshop", "cost_center_id": null, "weekly_contract_hours": 40,
                "cells": [
                  { "period_start": "2026-11-16", "contractual_hours": 40, "leave_and_holiday_hours": 8, "net_available_hours": 32,
                    "hard_booked_hours": 24, "soft_booked_hours": 12, "billable_booked_hours": 30, "internal_booked_hours": 6,
                    "load_percent": 112.5, "band": "red", "is_overloaded": true },
                  { "period_start": "2026-11-23", "contractual_hours": 0, "leave_and_holiday_hours": 0, "net_available_hours": 0,
                    "hard_booked_hours": 0, "soft_booked_hours": 0, "billable_booked_hours": 0, "internal_booked_hours": 0,
                    "load_percent": 0, "band": "none", "is_overloaded": false } ] } ]
            }
            """;

        var heatmap = ResourcePlanningJson.Read<CapacityHeatmap>(json);

        Assert.Equal(HeatmapGranularity.Week, heatmap.Granularity);
        Assert.Equal(80d, heatmap.Thresholds.Yellow);
        Assert.Equal(100d, heatmap.Thresholds.Red);
        Assert.Equal(2, heatmap.Periods.Count);
        var row = Assert.Single(heatmap.Rows);
        Assert.Equal("Workshop", row.DepartmentName);
        Assert.Equal(CapacityBand.Red, row.Cells[0].Band);
        Assert.True(row.Cells[0].IsOverloaded);
        Assert.Equal(32m, row.Cells[0].NetAvailableHours);
        Assert.Equal(CapacityBand.None, row.Cells[1].Band);
    }

    /// <summary>
    /// The mutation result is read with the allocations, the overload weeks and the warnings with their parameters.
    /// </summary>
    [Fact]
    public void Read_MutationResult_MapsOverloadAndWarnings()
    {
        var json = $$"""
            {
              "allocation": {{AllocationJson}},
              "allocations": [ {{AllocationJson}}, {{AllocationJson}} ],
              "overload": { "is_overloaded": true, "peak_load_percent": 125, "weeks": [ { "week_start": "2026-11-16", "load_percent": 125 } ] },
              "warnings": [ { "code": "skill_certificate_invalid", "params": { "skill": "Welding" } }, { "code": "overload", "params": null } ]
            }
            """;

        var result = ResourcePlanningJson.Read<AllocationMutationResult>(json);

        Assert.Equal(2, result.Allocations.Count);
        Assert.True(result.Overload.IsOverloaded);
        Assert.Equal(125d, result.Overload.PeakLoadPercent);
        Assert.Equal(new DateOnly(2026, 11, 16), Assert.Single(result.Overload.Weeks).WeekStart);
        Assert.Equal(2, result.Warnings.Count);
        Assert.Equal("skill_certificate_invalid", result.Warnings[0].Code);
        Assert.Equal("Welding", result.Warnings[0].Params!.Value.GetProperty("skill").GetString());
        Assert.Null(result.Warnings[1].Params);
    }

    /// <summary>
    /// A candidate is read with its matched skills, blockers and the certification status names.
    /// </summary>
    [Fact]
    public void Read_Candidate_MapsSkillsAndBlockers()
    {
        const string json = """
            [ { "employee_id": "22222222-2222-2222-2222-222222222222", "employee_name": "Bruno Bianchi", "department_name": null,
                "score": 61.5, "eligible": false,
                "matched_skills": [ { "skill_id": "55555555-5555-5555-5555-555555555555", "skill_name": "Welding", "level": 4, "min_level": 3, "valid_until_date": "2027-01-31", "certification_status": "not_required" } ],
                "blockers": [ { "code": "certificate_expired", "skill_id": "66666666-6666-6666-6666-666666666666", "params": null }, { "code": "insufficient_availability", "skill_id": null, "params": { "missing_hours": 12 } } ],
                "available_hours": 28, "requested_hours": 40, "residual_hours_per_day": 0, "peak_load_after_percent": 130, "hourly_cost": 62.5 } ]
            """;

        var candidates = ResourcePlanningJson.Read<IReadOnlyList<ResourceCandidate>>(json);

        var candidate = Assert.Single(candidates);
        Assert.False(candidate.Eligible);
        Assert.Equal(61.5d, candidate.Score);
        Assert.Equal(CertificationStatus.NotRequired, candidate.MatchedSkills[0].CertificationStatus);
        Assert.Equal(new DateOnly(2027, 1, 31), candidate.MatchedSkills[0].ValidUntilDate);
        Assert.Equal("certificate_expired", candidate.Blockers[0].Code);
        Assert.Equal(ResourcePlanningTestData.SkillCrane, candidate.Blockers[0].SkillId);
        Assert.Null(candidate.Blockers[1].SkillId);
        Assert.Equal(12, candidate.Blockers[1].Params!.Value.GetProperty("missing_hours").GetInt32());
        Assert.Equal(62.5m, candidate.HourlyCost);
    }

    /// <summary>
    /// The utilization report is read with its rows, its totals and the status names.
    /// </summary>
    [Fact]
    public void Read_UtilizationReport_MapsRowsAndStatus()
    {
        const string row = """
            { "key_id": null, "key_name": "Workshop", "net_available_hours": 320, "billable_committed_hours": 200, "billable_booked_hours": 240,
              "internal_booked_hours": 40, "tentative_booked_hours": 40, "actual_billable_percent": 62.5, "forecast_billable_percent": 75,
              "target_percent": 70, "gap_percent": -7.5, "status": "below_target" }
            """;
        var json = $$"""{ "from": "2026-11-01", "to": "2026-11-30", "group_by": "cost_center", "default_target_percent": 70, "rows": [ {{row}} ], "totals": {{row}} }""";

        var report = ResourcePlanningJson.Read<UtilizationReport>(json);

        Assert.Equal(UtilizationGroupBy.CostCenter, report.GroupBy);
        Assert.Equal(70d, report.DefaultTargetPercent);
        var item = Assert.Single(report.Rows);
        Assert.Equal(UtilizationStatus.BelowTarget, item.Status);
        Assert.Equal(-7.5d, item.GapPercent);
        Assert.Equal("Workshop", report.Totals.KeyName);
    }

    /// <summary>
    /// A skill profile is read with the certification status.
    /// </summary>
    [Fact]
    public void Read_SkillProfile_MapsCertificationStatus()
    {
        const string json = """
            { "id": "99999999-9999-9999-9999-999999999999", "employee_id": "11111111-1111-1111-1111-111111111111", "employee_name": "Anna Rossi",
              "skill_id": "55555555-5555-5555-5555-555555555555", "skill_name": "Welding", "category": "Workshop", "proficiency_level": 4,
              "valid_until_date": null, "is_valid": true, "requires_certification": true, "certification_code": "WELD-EN",
              "certification_status": "expired", "certification_valid_until": "2026-10-31" }
            """;

        var profile = ResourcePlanningJson.Read<EmployeeSkillProfile>(json);

        Assert.Equal(CertificationStatus.Expired, profile.CertificationStatus);
        Assert.Equal(new DateOnly(2026, 10, 31), profile.CertificationValidUntil);
        Assert.Equal(4, profile.ProficiencyLevel);
    }

    /// <summary>
    /// The capacity snapshot is read with its week and utilization rate.
    /// </summary>
    [Fact]
    public void Read_Snapshot_MapsTheWeek()
    {
        const string json = """
            { "id": "88888888-8888-8888-8888-888888888888", "employee_id": "11111111-1111-1111-1111-111111111111", "week_start_date": "2026-11-16",
              "contractual_hours": 40, "leave_and_holiday_hours": 8, "net_available_hours": 32, "billable_booked_hours": 20, "internal_booked_hours": 4,
              "utilization_rate_percent": 62.5, "department_id": null, "cost_center_id": null }
            """;

        var snapshot = ResourcePlanningJson.Read<ResourceCapacitySnapshot>(json);

        Assert.Equal(new DateOnly(2026, 11, 16), snapshot.WeekStartDate);
        Assert.Equal(62.5d, snapshot.UtilizationRatePercent);
    }

    /// <summary>
    /// The problem document of a refused request gives its invalid_params as issues; anything else gives none.
    /// </summary>
    [Fact]
    public void ReadIssues_ReadsInvalidParams()
    {
        const string problem = """{ "type": "about:blank", "title": "Unprocessable", "status": 422, "invalid_params": [ { "code": "hard_booking_requires_project", "params": {} }, { "code": "overload", "params": { "peak": 120 } } ] }""";

        var issues = ResourcePlanningJson.ReadIssues(problem);

        Assert.Equal(new[] { "hard_booking_requires_project", "overload" }, issues.Select(issue => issue.Code));
        Assert.Empty(ResourcePlanningJson.ReadIssues("{ \"title\": \"Server error\" }"));
        Assert.Empty(ResourcePlanningJson.ReadIssues("not json"));
        Assert.Empty(ResourcePlanningJson.ReadIssues(string.Empty));
    }

    /// <summary>
    /// The data member is cut out of the success envelope and an answer without it is rejected.
    /// </summary>
    [Fact]
    public void ReadData_ReturnsTheDataMember()
    {
        Assert.Equal("[1,2]", ResourcePlanningJson.ReadData("""{ "success": true, "message": "ok", "data": [1,2], "meta": {} }"""));
        _ = Assert.Throws<JsonException>(() => ResourcePlanningJson.ReadData("""{ "success": true }"""));
    }

    /// <summary>
    /// Enum values used in query strings are written in lower snake case.
    /// </summary>
    [Fact]
    public void Wire_UsesLowerSnakeCase()
    {
        Assert.Equal("cost_center", ResourcePlanningJson.Wire(UtilizationGroupBy.CostCenter));
        Assert.Equal("week", ResourcePlanningJson.Wire(HeatmapGranularity.Week));
        Assert.Equal("2026-11-02", ResourcePlanningJson.Day(new DateOnly(2026, 11, 2)));
    }
}
