// <copyright file="UtilizationTargetsViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using System.Net;
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
/// Tests of the utilization targets editor: the scopes (tenant default, departments, cost centers), the list, saving a target and the refusals.
/// </summary>
public sealed class UtilizationTargetsViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly Guid CostCenter = Guid.NewGuid();

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private IReadOnlyList<UtilizationTarget> targets;
    private PlanningCommandOutcome<UtilizationTarget> saved;

    /// <summary>
    /// Initializes a new instance of the <see cref="UtilizationTargetsViewModelTests"/> class.
    /// </summary>
    public UtilizationTargetsViewModelTests()
    {
        targets =
        [
            new UtilizationTarget(Guid.NewGuid(), null, null, 70d),
            new UtilizationTarget(Guid.NewGuid(), ResourcePlanningTestData.Department, null, 82.5d),
        ];
        saved = new PlanningCommandOutcome<UtilizationTarget>(HttpStatusCode.OK, targets[0], []);
        IReadOnlyList<PlanningResource> resources =
        [
            new PlanningResource(ResourcePlanningTestData.Anna, "Anna Rossi", ResourcePlanningTestData.Department, "Workshop", CostCenter, "CC Plant", 40m, 1m, 55m, "CH", "TI"),
            new PlanningResource(ResourcePlanningTestData.Bruno, "Bruno Bianchi", ResourcePlanningTestData.Department, "Workshop", CostCenter, "CC Plant", 40m, 1m, 55m, "CH", "TI"),
            new PlanningResource(Guid.NewGuid(), "Carla Verdi", null, null, null, null, 40m, 1m, 55m, "CH", "TI"),
        ];
        _ = sender.Setup(s => s.Send(It.IsAny<GetPlanningResourcesQuery>(), Any)).ReturnsAsync(resources);
        _ = sender.Setup(s => s.Send(It.IsAny<GetUtilizationTargetsQuery>(), Any)).ReturnsAsync(() => targets);
        _ = sender.Setup(s => s.Send(It.IsAny<SetUtilizationTargetCommand>(), Any)).ReturnsAsync(() => saved);
    }

    /// <summary>
    /// Loading offers the default of the tenant, each department and each cost center once, and lists the targets with their scope.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ListsTheScopesAndTheTargets()
    {
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal(
            new[]
            {
                loc["ResourcePlanning_Targets_Default"],
                string.Format(loc.CurrentCulture, loc["ResourcePlanning_Targets_Department"], "Workshop"),
                string.Format(loc.CurrentCulture, loc["ResourcePlanning_Targets_CostCenter"], "CC Plant"),
            },
            vm.ScopeChoices.Select(choice => choice.Label));
        Assert.True(vm.HasData);
        Assert.Equal(2, vm.Rows.Count);
        Assert.Contains(vm.Rows, row => row.ScopeText == loc["ResourcePlanning_Targets_Default"] && row.PercentText == "70%");
        Assert.Contains(vm.Rows, row => row.ScopeText.Contains("Workshop", StringComparison.Ordinal) && row.PercentText == 82.5d.ToString("0.#", loc.CurrentCulture) + "%");
    }

    /// <summary>
    /// The scopes are loaded once, not on every refresh.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_Twice_LoadsTheScopesOnce()
    {
        var vm = Create();

        await vm.LoadAsync();
        await vm.LoadAsync();

        sender.Verify(s => s.Send(It.IsAny<GetPlanningResourcesQuery>(), Any), Times.Once);
        Assert.Equal(3, vm.ScopeChoices.Count);
    }

    /// <summary>
    /// Without targets the editor says so.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_NoTargets_SaysSo()
    {
        targets = [];
        var vm = Create();

        await vm.LoadAsync();

        Assert.False(vm.HasData);
        Assert.Equal(loc["ResourcePlanning_Targets_Empty"], vm.StatusMessage);
    }

    /// <summary>
    /// Selecting a target fills the scope and the percentage of the form.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Select_FillsTheForm()
    {
        var vm = Create();
        await vm.LoadAsync();

        vm.SelectedRow = vm.Rows.Single(row => row.Target.DepartmentId == ResourcePlanningTestData.Department);

        Assert.Equal("D:" + ResourcePlanningTestData.Department.ToString("D"), vm.SelectedScope?.Key);
        Assert.Equal(82.5d.ToString("0.##", loc.CurrentCulture), vm.TargetPercentText);
    }

    /// <summary>
    /// Saving the default scope sets the tenant target; saving a department or a cost center sets that scope; the list is reloaded.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_SetsTheTargetOfTheScope()
    {
        var vm = Create();
        await vm.LoadAsync();

        vm.SelectedScope = vm.ScopeChoices[0];
        vm.TargetPercentText = 65m.ToString(loc.CurrentCulture);
        await vm.SaveAsync();
        vm.SelectedScope = vm.ScopeChoices[1];
        vm.TargetPercentText = 80.5m.ToString(loc.CurrentCulture);
        await vm.SaveAsync();
        vm.SelectedScope = vm.ScopeChoices[2];
        vm.TargetPercentText = 55m.ToString(loc.CurrentCulture);
        await vm.SaveAsync();

        sender.Verify(s => s.Send(It.Is<SetUtilizationTargetCommand>(c => c.Target == new UtilizationTargetRequest(null, null, 65d)), Any), Times.Once);
        sender.Verify(s => s.Send(It.Is<SetUtilizationTargetCommand>(c => c.Target == new UtilizationTargetRequest(ResourcePlanningTestData.Department, null, 80.5d)), Any), Times.Once);
        sender.Verify(s => s.Send(It.Is<SetUtilizationTargetCommand>(c => c.Target == new UtilizationTargetRequest(null, CostCenter, 55d)), Any), Times.Once);
        sender.Verify(s => s.Send(It.IsAny<GetUtilizationTargetsQuery>(), Any), Times.Exactly(4));
        Assert.Equal(loc["ResourcePlanning_Targets_Saved"], vm.StatusMessage);
    }

    /// <summary>
    /// A percentage that is missing, not a number or outside 0 to 100 is explained and not sent.
    /// </summary>
    /// <param name="text">The text typed in.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("101")]
    [InlineData("-5")]
    public async Task Save_InvalidPercent_IsExplainedAndNotSent(string text)
    {
        var vm = Create();
        vm.TargetPercentText = text;

        await vm.SaveAsync();

        Assert.Equal(loc["ResourcePlanning_Setup_target_invalid_percent"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<SetUtilizationTargetCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// A refusal of the server is shown with its violation.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_Refused_ShowsTheViolation()
    {
        saved = new PlanningCommandOutcome<UtilizationTarget>(HttpStatusCode.Forbidden, null, []);
        var vm = Create();
        vm.TargetPercentText = 70m.ToString(loc.CurrentCulture);

        await vm.SaveAsync();

        Assert.Equal(string.Format(loc.CurrentCulture, loc["ResourcePlanning_Error_Generic"], "403"), vm.StatusMessage);
    }

    /// <summary>
    /// A server that cannot be reached shows the save failure and the load failure.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ServerDown_ShowsTheFailures()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<SetUtilizationTargetCommand>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        _ = sender.Setup(s => s.Send(It.IsAny<GetUtilizationTargetsQuery>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        var vm = Create();
        vm.TargetPercentText = 70m.ToString(loc.CurrentCulture);

        await vm.SaveAsync();
        Assert.Equal(loc["ResourcePlanning_SaveFailed"], vm.StatusMessage);

        await vm.LoadAsync();
        Assert.Equal(loc["ResourcePlanning_LoadFailed"], vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    private UtilizationTargetsViewModel Create() => new(sender.Object, loc);
}
