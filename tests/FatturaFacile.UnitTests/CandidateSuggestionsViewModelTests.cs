// <copyright file="CandidateSuggestionsViewModelTests.cs" company="Fattura Facile">
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
/// Tests of the candidate suggestions view model: requirements, validation of the search and the display of eligible and blocked candidates.
/// </summary>
public sealed class CandidateSuggestionsViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private IReadOnlyList<ResourceCandidate> candidates;

    /// <summary>
    /// Initializes a new instance of the <see cref="CandidateSuggestionsViewModelTests"/> class.
    /// </summary>
    public CandidateSuggestionsViewModelTests()
    {
        IReadOnlyList<PlanningSkill> skills =
        [
            new PlanningSkill(ResourcePlanningTestData.SkillWelding, "Workshop", "Welding", true, "WELD-EN"),
            new PlanningSkill(ResourcePlanningTestData.SkillCrane, "Site", "Crane", false, null),
        ];
        candidates =
        [
            ResourcePlanningTestData.Candidate(ResourcePlanningTestData.Bruno, "Bruno Bianchi", eligible: true, 88.4d, 75d),
            ResourcePlanningTestData.Candidate(ResourcePlanningTestData.Anna, "Anna Rossi", eligible: false, 40d, 130d, new CandidateBlocker("certificate_expired", ResourcePlanningTestData.SkillWelding, null), new CandidateBlocker("insufficient_availability", null, null)),
        ];
        _ = sender.Setup(s => s.Send(It.IsAny<GetPlanningSkillsQuery>(), Any)).ReturnsAsync(skills);
        _ = sender.Setup(s => s.Send(It.IsAny<SearchResourceCandidatesQuery>(), Any)).ReturnsAsync(() => candidates);
    }

    /// <summary>
    /// The skills are listed by category and name, the first is selected and they are loaded only once.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ListsTheSkillsOnce()
    {
        var vm = Create();

        await vm.LoadAsync();
        await vm.LoadAsync();

        Assert.Equal(new[] { "Site / Crane", "Workshop / Welding" }, vm.SkillChoices.Select(choice => choice.Label));
        Assert.Equal(vm.SkillChoices[0], vm.SelectedSkill);
        sender.Verify(s => s.Send(It.IsAny<GetPlanningSkillsQuery>(), Any), Times.Once);
    }

    /// <summary>
    /// A requirement is added with its level, a skill added again is replaced and a requirement can be removed.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Requirements_AreAddedReplacedAndRemoved()
    {
        var vm = await LoadedAsync();
        vm.MinLevel = 4;
        vm.AddRequirement();
        vm.MinLevel = 2;
        vm.AddRequirement();

        var row = Assert.Single(vm.Requirements);
        Assert.Equal(2, row.MinLevel);
        Assert.Equal(ResourcePlanningTestData.SkillCrane, row.SkillId);

        vm.RemoveRequirement(row);
        vm.RemoveRequirement(null);

        Assert.Empty(vm.Requirements);
    }

    /// <summary>
    /// Adding without a skill selected explains why.
    /// </summary>
    [Fact]
    public void AddRequirement_WithoutSkill_Explains()
    {
        var vm = Create();

        vm.AddRequirement();

        Assert.Equal(loc["ResourcePlanning_Candidates_NeedSkill"], vm.StatusMessage);
        Assert.Empty(vm.Requirements);
    }

    /// <summary>
    /// A search without requirements, with an inverted period or with impossible hours is explained and not sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Search_InvalidInput_IsExplainedAndNotSent()
    {
        var vm = await LoadedAsync();

        await vm.SearchAsync();
        Assert.Equal(loc["ResourcePlanning_Candidates_NeedRequirement"], vm.StatusMessage);

        vm.AddRequirement();
        vm.EndDate = vm.StartDate.AddDays(-1);
        await vm.SearchAsync();
        Assert.Equal(loc["ResourcePlanning_Candidates_InvalidPeriod"], vm.StatusMessage);

        vm.EndDate = vm.StartDate;
        vm.HoursPerDayText = "30";
        await vm.SearchAsync();
        Assert.Equal(loc["ResourcePlanning_Candidates_InvalidHours"], vm.StatusMessage);

        vm.HoursPerDayText = "abc";
        await vm.SearchAsync();
        Assert.Equal(loc["ResourcePlanning_Candidates_InvalidHours"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<SearchResourceCandidatesQuery>(), Any), Times.Never);
    }

    /// <summary>
    /// The search sends the requirements, the period, the hours and the flags and shows the candidates in the order of the server.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Search_SendsTheCriteriaAndShowsTheCandidates()
    {
        var vm = await LoadedAsync();
        vm.SelectedSkill = vm.SkillChoices[1];
        vm.MinLevel = 3;
        vm.AddRequirement();
        vm.IncludeIneligible = true;

        await vm.SearchAsync();

        sender.Verify(
            s => s.Send(
                It.Is<SearchResourceCandidatesQuery>(q =>
                    q.Search.StartDate == new DateOnly(2026, 11, 21)
                    && q.Search.EndDate == new DateOnly(2026, 12, 4)
                    && q.Search.AllocatedHoursPerDay == 8m
                    && q.Search.Requirements.Count == 1
                    && q.Search.Requirements[0].SkillId == ResourcePlanningTestData.SkillWelding
                    && q.Search.Requirements[0].MinLevel == 3
                    && q.Search.RequireValidCertificates
                    && q.Search.IncludeIneligible
                    && q.Search.Limit == 10),
                Any),
            Times.Once);
        Assert.True(vm.HasData);
        Assert.Equal(new[] { "Bruno Bianchi", "Anna Rossi" }, vm.Candidates.Select(row => row.Candidate.EmployeeName));
        var best = vm.Candidates[0];
        Assert.Equal(loc["ResourcePlanning_Eligible"], best.EligibilityText);
        Assert.Equal("88", best.ScoreText);
        Assert.Equal("-", best.BlockersText);
        Assert.False(best.OverloadsAfter);
        Assert.Contains("Welding", best.MatchedText, StringComparison.Ordinal);
        var blocked = vm.Candidates[1];
        Assert.Equal(loc["ResourcePlanning_NotEligible"], blocked.EligibilityText);
        Assert.True(blocked.OverloadsAfter);
        Assert.Equal(loc["ResourcePlanning_Blocker_certificate_expired"] + "; " + loc["ResourcePlanning_Blocker_insufficient_availability"], blocked.BlockersText);
    }

    /// <summary>
    /// No candidate shows the notice.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Search_NoCandidate_ShowsTheNotice()
    {
        candidates = [];
        var vm = await LoadedAsync();
        vm.AddRequirement();

        await vm.SearchAsync();

        Assert.False(vm.HasData);
        Assert.Equal(loc["ResourcePlanning_Candidates_None"], vm.StatusMessage);
    }

    /// <summary>
    /// A server that cannot be reached shows the load failure.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Search_ServerDown_ShowsTheFailure()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<SearchResourceCandidatesQuery>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        var vm = await LoadedAsync();
        vm.AddRequirement();

        await vm.SearchAsync();

        Assert.Equal(loc["ResourcePlanning_LoadFailed"], vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    private async Task<CandidateSuggestionsViewModel> LoadedAsync()
    {
        var vm = Create();
        await vm.LoadAsync();

        return vm;
    }

    private CandidateSuggestionsViewModel Create() => new(sender.Object, loc, time);
}
