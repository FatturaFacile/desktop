// <copyright file="ResourcePlanningViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The resource planning and capacity saturation module (Mod 079): the capacity heatmap, the allocations, the candidate suggestions,
/// the billable utilization and the skill matrix, one tab each. A tab loads when it is first shown and on refresh.
/// </summary>
/// <param name="heatmap">The capacity heatmap.</param>
/// <param name="allocations">The allocations.</param>
/// <param name="candidates">The candidate suggestions.</param>
/// <param name="utilization">The billable utilization.</param>
/// <param name="skills">The skill matrix.</param>
internal sealed partial class ResourcePlanningViewModel(
    CapacityHeatmapViewModel heatmap,
    AllocationsViewModel allocations,
    CandidateSuggestionsViewModel candidates,
    BillableUtilizationViewModel utilization,
    SkillMatrixViewModel skills) : ObservableObject
{
    /// <summary>
    /// The index of the heatmap tab.
    /// </summary>
    public const int HeatmapTab = 0;

    /// <summary>
    /// The index of the allocations tab.
    /// </summary>
    public const int AllocationsTab = 1;

    /// <summary>
    /// The index of the candidate suggestions tab.
    /// </summary>
    public const int CandidatesTab = 2;

    /// <summary>
    /// The index of the billable utilization tab.
    /// </summary>
    public const int UtilizationTab = 3;

    /// <summary>
    /// The index of the skill matrix tab.
    /// </summary>
    public const int SkillsTab = 4;

    [ObservableProperty]
    private int _selectedTabIndex;

    /// <summary>
    /// Gets the capacity heatmap.
    /// </summary>
    public CapacityHeatmapViewModel Heatmap { get; } = heatmap;

    /// <summary>
    /// Gets the allocations.
    /// </summary>
    public AllocationsViewModel Allocations { get; } = allocations;

    /// <summary>
    /// Gets the candidate suggestions.
    /// </summary>
    public CandidateSuggestionsViewModel Candidates { get; } = candidates;

    /// <summary>
    /// Gets the billable utilization.
    /// </summary>
    public BillableUtilizationViewModel Utilization { get; } = utilization;

    /// <summary>
    /// Gets the skill matrix.
    /// </summary>
    public SkillMatrixViewModel Skills { get; } = skills;

    /// <summary>
    /// Loads the tab that is shown.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public Task LoadAsync() => SelectedTabIndex switch
    {
        AllocationsTab => Allocations.LoadAsync(),
        CandidatesTab => Candidates.LoadAsync(),
        UtilizationTab => Utilization.LoadAsync(),
        SkillsTab => Skills.LoadAsync(),
        _ => Heatmap.LoadAsync(),
    };

    partial void OnSelectedTabIndexChanged(int value) => _ = LoadAsync();
}
