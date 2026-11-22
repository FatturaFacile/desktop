// <copyright file="CandidateSearchRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The body of a candidate search.
/// </summary>
/// <param name="StartDate">The start date.</param>
/// <param name="EndDate">The end date.</param>
/// <param name="AllocatedHoursPerDay">The allocated hours per day.</param>
/// <param name="Requirements">The skills the candidate needs.</param>
/// <param name="ProjectId">The project id.</param>
/// <param name="WbsNodeId">The wbs node id.</param>
/// <param name="DepartmentId">The department id.</param>
/// <param name="RequireValidCertificates">Whether the certificates of the skills must be valid.</param>
/// <param name="IncludeIneligible">Whether candidates that do not qualify are listed with their blockers.</param>
/// <param name="Limit">The maximum number of candidates (up to 50).</param>
public sealed record CandidateSearchRequest(
    DateOnly StartDate,
    DateOnly EndDate,
    decimal AllocatedHoursPerDay,
    IReadOnlyList<CandidateRequirement> Requirements,
    Guid? ProjectId,
    Guid? WbsNodeId,
    Guid? DepartmentId,
    bool RequireValidCertificates,
    bool IncludeIneligible,
    int Limit);
