// <copyright file="SkillMatrixBuilderTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.ResourcePlanning;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the skill matrix: grouping by employee, ordering, the level filter and which skills count for staffing.
/// </summary>
public sealed class SkillMatrixBuilderTests
{
    /// <summary>
    /// Profiles are grouped by employee, employees and skills are sorted and the level filter drops weaker skills.
    /// </summary>
    [Fact]
    public void Build_GroupsSortsAndFilters()
    {
        var profiles = new[]
        {
            ResourcePlanningTestData.Profile(ResourcePlanningTestData.Bruno, "Bruno Bianchi", ResourcePlanningTestData.SkillWelding, "Welding", "Workshop", 2),
            ResourcePlanningTestData.Profile(ResourcePlanningTestData.Anna, "Anna Rossi", ResourcePlanningTestData.SkillWelding, "Welding", "Workshop", 4),
            ResourcePlanningTestData.Profile(ResourcePlanningTestData.Anna, "Anna Rossi", ResourcePlanningTestData.SkillCrane, "Crane", "Site", 3),
        };

        var all = SkillMatrixBuilder.Build(profiles, 1);
        var strong = SkillMatrixBuilder.Build(profiles, 3);

        Assert.Equal(new[] { "Anna Rossi", "Bruno Bianchi" }, all.Select(row => row.EmployeeName));
        Assert.Equal(new[] { "Crane", "Welding" }, all[0].Cells.Select(cell => cell.SkillName));
        var only = Assert.Single(strong);
        Assert.Equal("Anna Rossi", only.EmployeeName);
        Assert.Equal(2, only.Cells.Count);
    }

    /// <summary>
    /// A skill counts when the proficiency and any required certificate are valid; otherwise the state says why not.
    /// </summary>
    [Fact]
    public void StateOf_ExplainsWhyASkillDoesNotCount()
    {
        var valid = ResourcePlanningTestData.Profile(ResourcePlanningTestData.Anna, "Anna", ResourcePlanningTestData.SkillWelding, "Welding", "Workshop", 4, requiresCertification: true, certification: CertificationStatus.Valid);
        var expired = ResourcePlanningTestData.Profile(ResourcePlanningTestData.Anna, "Anna", ResourcePlanningTestData.SkillWelding, "Welding", "Workshop", 4, valid: false);
        var missing = ResourcePlanningTestData.Profile(ResourcePlanningTestData.Anna, "Anna", ResourcePlanningTestData.SkillWelding, "Welding", "Workshop", 4, requiresCertification: true, certification: CertificationStatus.Missing);
        var certificateExpired = ResourcePlanningTestData.Profile(ResourcePlanningTestData.Anna, "Anna", ResourcePlanningTestData.SkillWelding, "Welding", "Workshop", 4, requiresCertification: true, certification: CertificationStatus.Expired);
        var notRequired = ResourcePlanningTestData.Profile(ResourcePlanningTestData.Anna, "Anna", ResourcePlanningTestData.SkillWelding, "Welding", "Workshop", 4, certification: CertificationStatus.Missing);

        Assert.Equal(SkillMatrixCellState.Valid, SkillMatrixBuilder.StateOf(valid));
        Assert.Equal(SkillMatrixCellState.Expired, SkillMatrixBuilder.StateOf(expired));
        Assert.Equal(SkillMatrixCellState.CertificateMissing, SkillMatrixBuilder.StateOf(missing));
        Assert.Equal(SkillMatrixCellState.CertificateExpired, SkillMatrixBuilder.StateOf(certificateExpired));
        Assert.Equal(SkillMatrixCellState.Valid, SkillMatrixBuilder.StateOf(notRequired));
    }
}
