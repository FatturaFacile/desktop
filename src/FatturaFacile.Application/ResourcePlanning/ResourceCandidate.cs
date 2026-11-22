// <copyright file="ResourceCandidate.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// A suggested employee for a skill requirement and period.
/// </summary>
/// <param name="EmployeeId">The employee id.</param>
/// <param name="EmployeeName">The employee name.</param>
/// <param name="DepartmentName">The department name.</param>
/// <param name="Score">The fit from 0 to 100.</param>
/// <param name="Eligible">Whether the candidate meets every requirement.</param>
/// <param name="MatchedSkills">The matched skills.</param>
/// <param name="Blockers">The blockers.</param>
/// <param name="AvailableHours">The available hours.</param>
/// <param name="RequestedHours">The requested hours.</param>
/// <param name="ResidualHoursPerDay">The free hours per day after the booking.</param>
/// <param name="PeakLoadAfterPercent">The highest weekly load after the booking, in percent.</param>
/// <param name="HourlyCost">The hourly cost.</param>
public sealed record ResourceCandidate(
    Guid EmployeeId,
    string EmployeeName,
    string? DepartmentName,
    double Score,
    bool Eligible,
    IReadOnlyList<MatchedSkill> MatchedSkills,
    IReadOnlyList<CandidateBlocker> Blockers,
    decimal AvailableHours,
    decimal RequestedHours,
    decimal ResidualHoursPerDay,
    double PeakLoadAfterPercent,
    decimal HourlyCost);
