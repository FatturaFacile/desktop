// <copyright file="SkillMatrixViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using System.Net.Http;
using FatturaFacile.Application.Features.ResourcePlanning;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the skill matrix view model: rows per employee, the state of each skill and the level filter.
/// </summary>
public sealed class SkillMatrixViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private IReadOnlyList<EmployeeSkillProfile> profiles;

    /// <summary>
    /// Initializes a new instance of the <see cref="SkillMatrixViewModelTests"/> class.
    /// </summary>
    public SkillMatrixViewModelTests()
    {
        profiles =
        [
            ResourcePlanningTestData.Profile(ResourcePlanningTestData.Anna, "Anna Rossi", ResourcePlanningTestData.SkillWelding, "Welding", "Workshop", 4, requiresCertification: true, certification: CertificationStatus.Expired),
            ResourcePlanningTestData.Profile(ResourcePlanningTestData.Anna, "Anna Rossi", ResourcePlanningTestData.SkillCrane, "Crane", "Site", 2),
            ResourcePlanningTestData.Profile(ResourcePlanningTestData.Bruno, "Bruno Bianchi", ResourcePlanningTestData.SkillWelding, "Welding", "Workshop", 5, valid: false),
        ];
        _ = sender.Setup(s => s.Send(It.IsAny<GetEmployeeSkillProfilesQuery>(), Any)).ReturnsAsync(() => profiles);
    }

    /// <summary>
    /// The matrix shows one row per employee with the skills, their level and the state that explains the colour.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsTheSkillsWithTheirState()
    {
        var vm = new SkillMatrixViewModel(sender.Object, loc);

        await vm.LoadAsync();

        Assert.Equal(2, vm.Rows.Count);
        var anna = vm.Rows[0];
        Assert.Equal("Anna Rossi", anna.EmployeeName);
        Assert.Equal(new[] { "Valid", "CertificateExpired" }, anna.Cells.Select(cell => cell.StateName));
        Assert.Equal(string.Format(loc.CurrentCulture, loc["ResourcePlanning_Skill_Format"], "Welding", "4"), anna.Cells[1].Text);
        Assert.Contains(loc["ResourcePlanning_SkillState_CertificateExpired"], anna.Cells[1].ToolTip, StringComparison.Ordinal);
        Assert.Equal("Expired", vm.Rows[1].Cells[0].StateName);
        Assert.True(vm.HasData);
        Assert.Equal(string.Empty, vm.StatusMessage);
    }

    /// <summary>
    /// Raising the minimum level hides weaker skills and employees without any left, without asking the server again.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task MinimumLevel_FiltersTheLoadedProfiles()
    {
        var vm = new SkillMatrixViewModel(sender.Object, loc);
        await vm.LoadAsync();

        vm.MinimumLevel = 5;

        var only = Assert.Single(vm.Rows);
        Assert.Equal("Bruno Bianchi", only.EmployeeName);

        vm.MinimumLevel = 1;
        Assert.Equal(2, vm.Rows.Count);
        sender.Verify(s => s.Send(It.IsAny<GetEmployeeSkillProfilesQuery>(), Any), Times.Once);
    }

    /// <summary>
    /// Without profiles the empty notice is shown.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_NoProfiles_ShowsTheEmptyText()
    {
        profiles = [];
        var vm = new SkillMatrixViewModel(sender.Object, loc);

        await vm.LoadAsync();

        Assert.False(vm.HasData);
        Assert.Equal(loc["ResourcePlanning_Skills_Empty"], vm.StatusMessage);
    }

    /// <summary>
    /// A server that cannot be reached shows the load failure.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ServerDown_ShowsTheFailure()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetEmployeeSkillProfilesQuery>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        var vm = new SkillMatrixViewModel(sender.Object, loc);

        await vm.LoadAsync();

        Assert.Equal(loc["ResourcePlanning_LoadFailed"], vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }
}
