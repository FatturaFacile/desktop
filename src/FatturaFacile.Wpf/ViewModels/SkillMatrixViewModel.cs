// <copyright file="SkillMatrixViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.ResourcePlanning;
using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the skill matrix: the skills and proficiency of each employee, marking the skills that do not count for staffing because
/// the proficiency has expired or a required certificate is missing or expired.
/// </summary>
internal sealed partial class SkillMatrixViewModel : ObservableObject
{
    private const int MaxLevel = 5;

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly ResourcePlanningFormatter _format;

    [ObservableProperty]
    private int _minimumLevel = 1;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    private IReadOnlyList<EmployeeSkillProfile> _profiles = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="SkillMatrixViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    public SkillMatrixViewModel(ISender sender, ILocalizationService localization)
    {
        _sender = sender;
        _localization = localization;
        _format = new ResourcePlanningFormatter(localization);
    }

    /// <summary>
    /// Gets the proficiency levels the matrix can be limited to.
    /// </summary>
    public IReadOnlyList<int> Levels { get; } = [.. Enumerable.Range(1, MaxLevel)];

    /// <summary>
    /// Gets the employee rows.
    /// </summary>
    public ObservableCollection<SkillMatrixRowItem> Rows { get; } = [];

    /// <summary>
    /// Loads the skill profiles of the employees.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            _profiles = await _sender.Send(new GetEmployeeSkillProfilesQuery(null, null));
            Rebuild();
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
        catch (HttpRequestException)
        {
            StatusMessage = _localization["ResourcePlanning_LoadFailed"];
        }
        catch (JsonException)
        {
            StatusMessage = _localization["ResourcePlanning_LoadFailed"];
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnMinimumLevelChanged(int value) => Rebuild();

    private void Rebuild()
    {
        Rows.Clear();
        var culture = _format.Culture;

        foreach (var row in SkillMatrixBuilder.Build(_profiles, MinimumLevel))
        {
            Rows.Add(new SkillMatrixRowItem(
                row.EmployeeName,
                [.. row.Cells.Select(cell => new SkillMatrixCellItem(
                    string.Format(culture, _localization["ResourcePlanning_Skill_Format"], cell.SkillName, cell.Level.ToString(culture)),
                    cell.State.ToString(),
                    cell.Category + " - " + _localization["ResourcePlanning_SkillState_" + cell.State]))]));
        }

        HasData = Rows.Count > 0;
        StatusMessage = HasData ? string.Empty : _localization["ResourcePlanning_Skills_Empty"];
    }
}
