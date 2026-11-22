// <copyright file="CandidateSuggestionsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
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
/// Drives the candidate suggestions: the planner lists the skills and proficiency a booking needs, a period and the hours per day,
/// and the server ranks the employees that fit with the reasons why the others do not.
/// </summary>
internal sealed partial class CandidateSuggestionsViewModel : ObservableObject
{
    private const int PeriodDays = 14;
    private const int DefaultMinLevel = 3;
    private const int MaxLevel = 5;
    private const int SearchLimit = 10;
    private const decimal HoursPerDayMax = 24m;
    private const string DefaultHoursPerDay = "8";
    private const string NumberFormat = "0.##";

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly ResourcePlanningFormatter _format;

    [ObservableProperty]
    private DateTime _startDate;

    [ObservableProperty]
    private DateTime _endDate;

    [ObservableProperty]
    private string _hoursPerDayText = DefaultHoursPerDay;

    [ObservableProperty]
    private PlanningChoice? _selectedSkill;

    [ObservableProperty]
    private int _minLevel = DefaultMinLevel;

    [ObservableProperty]
    private bool _requireValidCertificates = true;

    [ObservableProperty]
    private bool _includeIneligible;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CandidateSuggestionsViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    public CandidateSuggestionsViewModel(ISender sender, ILocalizationService localization, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(time);

        _sender = sender;
        _localization = localization;
        _format = new ResourcePlanningFormatter(localization);
        var today = time.GetLocalNow().Date;
        _startDate = today;
        _endDate = today.AddDays(PeriodDays - 1);
    }

    /// <summary>
    /// Gets the proficiency levels a requirement can ask for.
    /// </summary>
    public IReadOnlyList<int> Levels { get; } = [.. Enumerable.Range(1, MaxLevel)];

    /// <summary>
    /// Gets the skills of the taxonomy.
    /// </summary>
    public ObservableCollection<PlanningChoice> SkillChoices { get; } = [];

    /// <summary>
    /// Gets the skills the candidate needs.
    /// </summary>
    public ObservableCollection<RequirementRow> Requirements { get; } = [];

    /// <summary>
    /// Gets the suggested employees, best first.
    /// </summary>
    public ObservableCollection<CandidateRow> Candidates { get; } = [];

    /// <summary>
    /// Loads the skills of the taxonomy.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        if (SkillChoices.Count > 0)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var skills = await _sender.Send(new GetPlanningSkillsQuery(null));
            foreach (var skill in skills.OrderBy(item => item.Category, StringComparer.CurrentCultureIgnoreCase).ThenBy(item => item.SkillName, StringComparer.CurrentCultureIgnoreCase))
            {
                SkillChoices.Add(new PlanningChoice(skill.Id.ToString("D", CultureInfo.InvariantCulture), skill.Category + " / " + skill.SkillName));
            }

            SelectedSkill = SkillChoices.Count > 0 ? SkillChoices[0] : null;
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

    /// <summary>
    /// Adds the selected skill with the minimum proficiency to the requirements; a skill that is already listed is replaced.
    /// </summary>
    [RelayCommand]
    public void AddRequirement()
    {
        if (SelectedSkill is null || !Guid.TryParse(SelectedSkill.Key, out var skillId))
        {
            StatusMessage = _localization["ResourcePlanning_Candidates_NeedSkill"];

            return;
        }

        var existing = Requirements.FirstOrDefault(row => row.SkillId == skillId);
        if (existing is not null)
        {
            _ = Requirements.Remove(existing);
        }

        Requirements.Add(new RequirementRow(skillId, SelectedSkill.Label, MinLevel, string.Format(_format.Culture, _localization["ResourcePlanning_Requirement_Format"], SelectedSkill.Label, MinLevel.ToString(_format.Culture))));
        StatusMessage = string.Empty;
    }

    /// <summary>
    /// Removes a requirement.
    /// </summary>
    /// <param name="row">The requirement to remove.</param>
    [RelayCommand]
    public void RemoveRequirement(RequirementRow? row)
    {
        if (row is not null)
        {
            _ = Requirements.Remove(row);
        }
    }

    /// <summary>
    /// Asks the server for the employees that fit the requirements over the period.
    /// </summary>
    /// <returns>A task representing the search.</returns>
    [RelayCommand]
    public async Task SearchAsync()
    {
        var problem = Validate(out var hours);
        if (problem is not null)
        {
            StatusMessage = _localization[problem];

            return;
        }

        IsBusy = true;

        try
        {
            var search = new CandidateSearchRequest(
                DateOnly.FromDateTime(StartDate),
                DateOnly.FromDateTime(EndDate),
                hours,
                [.. Requirements.Select(row => new CandidateRequirement(row.SkillId, row.MinLevel))],
                null,
                null,
                null,
                RequireValidCertificates,
                IncludeIneligible,
                SearchLimit);
            var candidates = await _sender.Send(new SearchResourceCandidatesQuery(search));
            Candidates.Clear();
            foreach (var candidate in candidates)
            {
                Candidates.Add(ToRow(candidate));
            }

            HasData = Candidates.Count > 0;
            StatusMessage = HasData ? string.Empty : _localization["ResourcePlanning_Candidates_None"];
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

    private string? Validate(out decimal hours)
    {
        hours = _format.ParseDecimal(HoursPerDayText) ?? 0m;

        if (Requirements.Count == 0)
        {
            return "ResourcePlanning_Candidates_NeedRequirement";
        }

        if (EndDate < StartDate)
        {
            return "ResourcePlanning_Candidates_InvalidPeriod";
        }

        return hours <= 0m || hours > HoursPerDayMax ? "ResourcePlanning_Candidates_InvalidHours" : null;
    }

    private CandidateRow ToRow(ResourceCandidate candidate)
    {
        var culture = _format.Culture;
        var matched = string.Join(", ", candidate.MatchedSkills.Select(skill => string.Format(culture, _localization["ResourcePlanning_Matched_Format"], skill.SkillName, skill.Level.ToString(culture), skill.MinLevel.ToString(culture))));
        var blockers = string.Join("; ", candidate.Blockers.Select(blocker => _format.Describe("Blocker", blocker.Code)));

        return new CandidateRow(
            candidate,
            candidate.Score.ToString("0", culture),
            _localization[candidate.Eligible ? "ResourcePlanning_Eligible" : "ResourcePlanning_NotEligible"],
            ResourcePlanningFormatter.OrDash(matched),
            ResourcePlanningFormatter.OrDash(blockers),
            string.Format(culture, _localization["ResourcePlanning_Availability_Format"], _format.Hours(candidate.AvailableHours), _format.Hours(candidate.RequestedHours), candidate.ResidualHoursPerDay.ToString(NumberFormat, culture)),
            _format.Percent(candidate.PeakLoadAfterPercent),
            CapacityBands.IsOverload(candidate.PeakLoadAfterPercent),
            candidate.HourlyCost.ToString("0.00", culture));
    }
}
