// <copyright file="SkillTaxonomyViewModelTests.cs" company="Fattura Facile">
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
/// Tests of the skill taxonomy view model: the list, creating, updating and deleting skills (with its confirmation), the checks of the form
/// and the refusals of the server.
/// </summary>
public sealed class SkillTaxonomyViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly PlanningSkill welding = new(ResourcePlanningTestData.SkillWelding, "Site", "Welding", true, "WELD-EN");
    private readonly PlanningSkill crane = new(ResourcePlanningTestData.SkillCrane, "Site", "Crane", false, null);
    private readonly PlanningSkill design = new(Guid.NewGuid(), "Office", "CAD design", false, null);
    private IReadOnlyList<PlanningSkill> skills;
    private PlanningCommandOutcome<PlanningSkill> saved;
    private PlanningCommandOutcome<bool> deleted = new(HttpStatusCode.NoContent, true, []);

    /// <summary>
    /// Initializes a new instance of the <see cref="SkillTaxonomyViewModelTests"/> class.
    /// </summary>
    public SkillTaxonomyViewModelTests()
    {
        skills = [welding, design, crane];
        saved = new PlanningCommandOutcome<PlanningSkill>(HttpStatusCode.Created, crane, []);
        _ = sender.Setup(s => s.Send(It.IsAny<GetPlanningSkillsQuery>(), Any)).ReturnsAsync(() => skills);
        _ = sender.Setup(s => s.Send(It.IsAny<CreatePlanningSkillCommand>(), Any)).ReturnsAsync(() => saved);
        _ = sender.Setup(s => s.Send(It.IsAny<UpdatePlanningSkillCommand>(), Any)).ReturnsAsync(() => saved);
        _ = sender.Setup(s => s.Send(It.IsAny<DeletePlanningSkillCommand>(), Any)).ReturnsAsync(() => deleted);
    }

    /// <summary>
    /// The list shows the skills by category and name with the certificate they need, or a dash.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsTheSkillsByCategoryAndName()
    {
        var vm = Create();

        await vm.LoadAsync();

        Assert.True(vm.HasData);
        Assert.Equal(new[] { "CAD design", "Crane", "Welding" }, vm.Rows.Select(row => row.Skill.SkillName));
        Assert.Equal(new[] { "-", "-", "WELD-EN" }, vm.Rows.Select(row => row.CertificationText));
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// An empty taxonomy is said so.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_Empty_SaysSo()
    {
        skills = [];
        var vm = Create();

        await vm.LoadAsync();

        Assert.False(vm.HasData);
        Assert.Equal(loc["ResourcePlanning_Taxonomy_Empty"], vm.StatusMessage);
    }

    /// <summary>
    /// A list that cannot be loaded shows the load failure.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ServerDown_ShowsTheLoadFailure()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetPlanningSkillsQuery>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        var vm = Create();

        await vm.LoadAsync();

        Assert.Equal(loc["ResourcePlanning_LoadFailed"], vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// Selecting a skill fills the form with it and switches to editing; a new skill clears the form again.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Select_FillsTheFormAndStartNewClearsIt()
    {
        var vm = Create();
        await vm.LoadAsync();

        vm.SelectedRow = vm.Rows.Single(row => row.Skill.SkillName == "Welding");

        Assert.True(vm.IsEditing);
        Assert.Equal("Site", vm.CategoryText);
        Assert.Equal("Welding", vm.SkillNameText);
        Assert.True(vm.RequiresCertification);
        Assert.Equal("WELD-EN", vm.CertificationCodeText);

        vm.StartNew();

        Assert.False(vm.IsEditing);
        Assert.Equal(string.Empty, vm.CategoryText);
        Assert.Equal(string.Empty, vm.SkillNameText);
        Assert.False(vm.RequiresCertification);
        Assert.Equal(string.Empty, vm.CertificationCodeText);
    }

    /// <summary>
    /// Saving without a selection creates the skill, reloads the list and selects the new skill.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_New_CreatesTheSkill()
    {
        var vm = Create();
        await vm.LoadAsync();
        vm.StartNew();
        vm.CategoryText = " Site ";
        vm.SkillNameText = "Crane";
        vm.RequiresCertification = false;

        await vm.SaveAsync();

        sender.Verify(s => s.Send(It.Is<CreatePlanningSkillCommand>(c => c.Skill == new PlanningSkillRequest("Site", "Crane", false, null)), Any), Times.Once);
        sender.Verify(s => s.Send(It.IsAny<UpdatePlanningSkillCommand>(), Any), Times.Never);
        sender.Verify(s => s.Send(It.IsAny<GetPlanningSkillsQuery>(), Any), Times.Exactly(2));
        Assert.Equal(loc["ResourcePlanning_Taxonomy_Saved"], vm.StatusMessage);
        Assert.Equal(ResourcePlanningTestData.SkillCrane, vm.SelectedRow?.Skill.Id);
    }

    /// <summary>
    /// Saving with a selection updates that skill and keeps it selected.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_Selected_UpdatesTheSkill()
    {
        saved = new PlanningCommandOutcome<PlanningSkill>(HttpStatusCode.OK, welding, []);
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedRow = vm.Rows.Single(row => row.Skill.SkillName == "Welding");
        vm.CertificationCodeText = "WELD-ISO";

        await vm.SaveAsync();

        sender.Verify(s => s.Send(It.Is<UpdatePlanningSkillCommand>(c => c.SkillId == ResourcePlanningTestData.SkillWelding && c.Skill == new PlanningSkillRequest("Site", "Welding", true, "WELD-ISO")), Any), Times.Once);
        sender.Verify(s => s.Send(It.IsAny<CreatePlanningSkillCommand>(), Any), Times.Never);
        Assert.Equal(ResourcePlanningTestData.SkillWelding, vm.SelectedRow?.Skill.Id);
    }

    /// <summary>
    /// A form that is not valid is explained and not sent.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="name">The name.</param>
    /// <param name="requires">Whether a certificate is required.</param>
    /// <param name="code">The certificate type.</param>
    /// <param name="expected">The expected problem code.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData("", "Crane", false, "", PlanningDraftBuilder.SkillNeedsCategory)]
    [InlineData("Site", " ", false, "", PlanningDraftBuilder.SkillNeedsName)]
    [InlineData("Site", "Crane", true, "", PlanningDraftBuilder.CertificationCodeRequired)]
    public async Task Save_InvalidForm_IsExplainedAndNotSent(string category, string name, bool requires, string code, string expected)
    {
        var vm = Create();
        vm.CategoryText = category;
        vm.SkillNameText = name;
        vm.RequiresCertification = requires;
        vm.CertificationCodeText = code;

        await vm.SaveAsync();

        Assert.Equal(loc["ResourcePlanning_Setup_" + expected], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<CreatePlanningSkillCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// A refusal of the server is shown with its violation and the list is not reloaded.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_Refused_ShowsTheViolation()
    {
        saved = new PlanningCommandOutcome<PlanningSkill>(HttpStatusCode.UnprocessableEntity, null, [new PlanningIssue("certification_code_required", null)]);
        var vm = Create();
        vm.CategoryText = "Site";
        vm.SkillNameText = "Crane";

        await vm.SaveAsync();

        Assert.Equal(loc["ResourcePlanning_Error_certification_code_required"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<GetPlanningSkillsQuery>(), Any), Times.Never);
    }

    /// <summary>
    /// A server that cannot be reached shows the save failure and the view stops being busy.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_ServerDown_ShowsTheSaveFailure()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<CreatePlanningSkillCommand>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        var vm = Create();
        vm.CategoryText = "Site";
        vm.SkillNameText = "Crane";

        await vm.SaveAsync();

        Assert.Equal(loc["ResourcePlanning_SaveFailed"], vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// Deleting asks for a confirmation first; confirming deletes the skill, clears the form and reloads the list.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Delete_NeedsAConfirmation()
    {
        var vm = Create();
        await vm.LoadAsync();
        vm.RequestDelete();
        Assert.Equal(loc["ResourcePlanning_Taxonomy_NeedsSelection"], vm.StatusMessage);

        vm.SelectedRow = vm.Rows.Single(row => row.Skill.SkillName == "Crane");
        vm.RequestDelete();

        Assert.True(vm.IsDeletePending);
        Assert.Equal(string.Format(loc.CurrentCulture, loc["ResourcePlanning_Taxonomy_DeleteConfirm"], "Crane"), vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<DeletePlanningSkillCommand>(), Any), Times.Never);

        skills = [welding, design];
        await vm.ConfirmDeleteAsync();

        sender.Verify(s => s.Send(It.Is<DeletePlanningSkillCommand>(c => c.SkillId == ResourcePlanningTestData.SkillCrane), Any), Times.Once);
        Assert.Equal(2, vm.Rows.Count);
        Assert.Null(vm.SelectedRow);
        Assert.Equal(string.Empty, vm.SkillNameText);
        Assert.False(vm.IsDeletePending);
        Assert.Equal(loc["ResourcePlanning_Taxonomy_Deleted"], vm.StatusMessage);
    }

    /// <summary>
    /// Cancelling, selecting another skill, or confirming without a request deletes nothing.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Delete_CancelledOrUnrequested_DeletesNothing()
    {
        var vm = Create();
        await vm.LoadAsync();
        await vm.ConfirmDeleteAsync();
        vm.SelectedRow = vm.Rows[0];
        vm.RequestDelete();
        vm.CancelDelete();
        await vm.ConfirmDeleteAsync();
        vm.RequestDelete();
        vm.SelectedRow = vm.Rows[1];
        await vm.ConfirmDeleteAsync();

        Assert.False(vm.IsDeletePending);
        sender.Verify(s => s.Send(It.IsAny<DeletePlanningSkillCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// A refused deletion shows the violation and keeps the skill selected.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Delete_Refused_ShowsTheViolation()
    {
        deleted = new PlanningCommandOutcome<bool>(HttpStatusCode.NotFound, false, [new PlanningIssue("skill_not_found", null)]);
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedRow = vm.Rows[0];
        vm.RequestDelete();

        await vm.ConfirmDeleteAsync();

        Assert.Equal(loc["ResourcePlanning_Error_skill_not_found"], vm.StatusMessage);
        Assert.NotNull(vm.SelectedRow);
    }

    private SkillTaxonomyViewModel Create() => new(sender.Object, loc);
}
