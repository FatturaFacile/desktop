// <copyright file="SkillMatrixBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// Groups the skill profiles by employee into the rows of the skill matrix and tells which skills count for staffing.
/// </summary>
public static class SkillMatrixBuilder
{
    /// <summary>
    /// Builds the matrix.
    /// </summary>
    /// <param name="profiles">The profiles.</param>
    /// <param name="minimumLevel">Only skills at or above this proficiency are kept (1 keeps every skill).</param>
    /// <returns>One row per employee with at least one skill, by name.</returns>
    public static IReadOnlyList<SkillMatrixRow> Build(IEnumerable<EmployeeSkillProfile> profiles, int minimumLevel)
    {
        ArgumentNullException.ThrowIfNull(profiles);

        return
        [
            .. profiles
                .Where(profile => profile.ProficiencyLevel >= minimumLevel)
                .GroupBy(profile => profile.EmployeeId)
                .Select(group => new SkillMatrixRow(
                    group.Key,
                    group.First().EmployeeName,
                    [
                        .. group
                            .OrderBy(profile => profile.Category, StringComparer.CurrentCultureIgnoreCase)
                            .ThenBy(profile => profile.SkillName, StringComparer.CurrentCultureIgnoreCase)
                            .Select(profile => new SkillMatrixCell(profile.SkillId, profile.SkillName, profile.Category, profile.ProficiencyLevel, StateOf(profile))),
                    ]))
                .OrderBy(row => row.EmployeeName, StringComparer.CurrentCultureIgnoreCase),
        ];
    }

    /// <summary>
    /// Tells whether a profile counts for staffing.
    /// </summary>
    /// <param name="profile">The profile.</param>
    /// <returns>The state: an expired certificate or proficiency, a missing certificate, or valid.</returns>
    public static SkillMatrixCellState StateOf(EmployeeSkillProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (profile.RequiresCertification)
        {
            if (profile.CertificationStatus == CertificationStatus.Missing)
            {
                return SkillMatrixCellState.CertificateMissing;
            }

            if (profile.CertificationStatus == CertificationStatus.Expired)
            {
                return SkillMatrixCellState.CertificateExpired;
            }
        }

        return profile.IsValid ? SkillMatrixCellState.Valid : SkillMatrixCellState.Expired;
    }
}
