// <copyright file="ResourcePlanningTestData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>
#pragma warning disable SA1600 // Elements should be documented: internal test data.

using System.Diagnostics.CodeAnalysis;
using FatturaFacile.Application.ResourcePlanning;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Builders of the Mod 079 models shared by the resource planning tests.
/// </summary>
internal static class ResourcePlanningTestData
{
    public static readonly Guid Anna = new(0x11111111, 0x1111, 0x1111, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11);
    public static readonly Guid Bruno = new(0x22222222, 0x2222, 0x2222, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22);
    public static readonly Guid ProjectA = new(0x33333333, 0x3333, 0x3333, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33, 0x33);
    public static readonly Guid Opportunity = new(0x44444444, 0x4444, 0x4444, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44, 0x44);
    public static readonly Guid SkillWelding = new(0x55555555, 0x5555, 0x5555, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55);
    public static readonly Guid SkillCrane = new(0x66666666, 0x6666, 0x6666, 0x66, 0x66, 0x66, 0x66, 0x66, 0x66, 0x66, 0x66);
    public static readonly Guid Department = new(0x77777777, 0x7777, 0x7777, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77, 0x77);
    public static readonly Guid AllocationOne = new(0x88888888, 0x8888, 0x8888, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x88);
    public static readonly Guid AllocationTwo = new(0x99999999, 0x9999, 0x9999, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99);

    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "A test builder whose optional parameters mirror the fields a test varies.")]
    public static ResourceAllocation Allocation(
        Guid id,
        ResourceBookingType type,
        Guid? projectId = null,
        Guid? employeeId = null,
        string employeeName = "Anna Rossi",
        string? projectName = "Plant retrofit",
        Guid? opportunityId = null,
        string? opportunityName = null,
        bool overloaded = false,
        double peak = 70d) =>
        new(
            id,
            projectId,
            projectName,
            WbsNodeId: null,
            WbsNodeTitle: null,
            employeeId ?? Anna,
            employeeName,
            RoleId: null,
            type,
            new DateOnly(2026, 11, 16),
            new DateOnly(2026, 11, 27),
            6m,
            60m,
            IsBillable: true,
            opportunityId,
            opportunityName,
            overloaded,
            peak,
            Notes: null);

    public static OverloadSummary NoOverload() => new(IsOverloaded: false, 70d, []);

    public static AllocationMutationResult Mutation(ResourceAllocation allocation, OverloadSummary? overload = null, params PlanningIssue[] warnings) =>
        new(allocation, [allocation], overload ?? NoOverload(), warnings);

    public static HeatmapCell Cell(DateOnly start, decimal net, decimal hard, decimal soft, double load, CapacityBand band) =>
        new(start, 40m, 40m - net, net, hard, soft, hard, soft, load, band, band == CapacityBand.Red);

    public static CapacityHeatmap Heatmap(params HeatmapRow[] rows) =>
        new(
            new DateOnly(2026, 11, 16),
            new DateOnly(2026, 12, 6),
            HeatmapGranularity.Week,
            new HeatmapThresholds(80d, 100d),
            [new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 23), new DateOnly(2026, 11, 30)],
            rows);

    public static HeatmapRow Row(Guid employeeId, string name, string? department, params HeatmapCell[] cells) =>
        new(employeeId, name, DepartmentId: null, department, CostCenterId: null, 40m, cells);

    public static PlanningResource Resource(Guid employeeId, string name, Guid? departmentId = null, string? department = null) =>
        new(employeeId, name, departmentId, department, CostCenterId: null, CostCenterName: null, 40m, 1m, 55m, "CH", "TI");

    [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "A test builder whose optional parameters mirror the fields a test varies.")]
    public static EmployeeSkillProfile Profile(
        Guid employeeId,
        string employee,
        Guid skillId,
        string skill,
        string category,
        int level,
        bool valid = true,
        bool requiresCertification = false,
        CertificationStatus certification = CertificationStatus.NotRequired) =>
        new(Guid.NewGuid(), employeeId, employee, skillId, skill, category, level, ValidUntilDate: null, valid, requiresCertification, requiresCertification ? "WELD-EN" : null, certification, CertificationValidUntil: null);

    public static ResourceCandidate Candidate(Guid employeeId, string name, bool eligible, double score, double peakAfter, params CandidateBlocker[] blockers) =>
        new(
            employeeId,
            name,
            "Workshop",
            score,
            eligible,
            [new MatchedSkill(SkillWelding, "Welding", 4, 3, ValidUntilDate: null, CertificationStatus.Valid)],
            blockers,
            32m,
            40m,
            3.5m,
            peakAfter,
            55m);

    public static UtilizationRow Utilization(string name, double actual, double forecast, double target, UtilizationStatus status) =>
        new(KeyId: null, name, 160m, 100m, 120m, 20m, 10m, actual, forecast, target, actual - target, status);
}
