// <copyright file="SkillTaxonomyViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.ResourcePlanning;
using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the skill taxonomy: the skills the staffing is matched on are listed, created, edited and deleted (after a confirmation).
/// A skill can require a valid certificate of a given type to count.
/// </summary>
internal sealed partial class SkillTaxonomyViewModel : ObservableObject
{
    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly ResourcePlanningFormatter _format;

    [ObservableProperty]
    private SkillTaxonomyRow? _selectedRow;

    [ObservableProperty]
    private string _categoryText = string.Empty;

    [ObservableProperty]
    private string _skillNameText = string.Empty;

    [ObservableProperty]
    private bool _requiresCertification;

    [ObservableProperty]
    private string _certificationCodeText = string.Empty;

    [ObservableProperty]
    private bool _isDeletePending;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="SkillTaxonomyViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    public SkillTaxonomyViewModel(ISender sender, ILocalizationService localization)
    {
        _sender = sender;
        _localization = localization;
        _format = new ResourcePlanningFormatter(localization);
    }

    /// <summary>
    /// Gets the skills of the taxonomy.
    /// </summary>
    public ObservableCollection<SkillTaxonomyRow> Rows { get; } = [];

    /// <summary>
    /// Gets a value indicating whether a skill is selected, so that saving updates it instead of creating a new one.
    /// </summary>
    public bool IsEditing => SelectedRow is not null;

    /// <summary>
    /// Loads the skills.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        var failure = await PlanningCalls.ReadAsync(() => ReloadAsync(SelectedRow?.Skill.Id), _localization);
        StatusMessage = failure ?? StatusMessage;
        IsBusy = false;
    }

    /// <summary>
    /// Clears the form to describe a new skill.
    /// </summary>
    [RelayCommand]
    public void StartNew()
    {
        SelectedRow = null;
        Clear();
    }

    /// <summary>
    /// Saves the form: a new skill is created, the selected one is updated.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand]
    public async Task SaveAsync()
    {
        var built = PlanningDraftBuilder.Skill(CategoryText, SkillNameText, RequiresCertification, CertificationCodeText);
        if (built.Request is not { } request)
        {
            StatusMessage = _format.Describe("Setup", built.ErrorCode ?? string.Empty);

            return;
        }

        IsBusy = true;
        var existing = SelectedRow?.Skill.Id;
        var result = await PlanningCalls.WriteAsync(
            () => existing is { } id ? _sender.Send(new UpdatePlanningSkillCommand(id, request)) : _sender.Send(new CreatePlanningSkillCommand(request)),
            _format,
            _localization);
        if (result.IsSuccess)
        {
            StatusMessage = _localization["ResourcePlanning_Taxonomy_Saved"];
            await ReloadAfterWriteAsync(result.Value?.Id ?? existing);
        }
        else
        {
            StatusMessage = result.Message;
        }

        IsBusy = false;
    }

    /// <summary>
    /// Asks the user to confirm the deletion of the selected skill; nothing is deleted yet.
    /// </summary>
    [RelayCommand]
    public void RequestDelete()
    {
        if (SelectedRow is null)
        {
            StatusMessage = _localization["ResourcePlanning_Taxonomy_NeedsSelection"];

            return;
        }

        IsDeletePending = true;
        StatusMessage = string.Format(_format.Culture, _localization["ResourcePlanning_Taxonomy_DeleteConfirm"], SelectedRow.Skill.SkillName);
    }

    /// <summary>
    /// Withdraws the request to delete the selected skill.
    /// </summary>
    [RelayCommand]
    public void CancelDelete()
    {
        IsDeletePending = false;
        StatusMessage = string.Empty;
    }

    /// <summary>
    /// Deletes the selected skill once the user confirmed it.
    /// </summary>
    /// <returns>A task representing the deletion.</returns>
    [RelayCommand]
    public async Task ConfirmDeleteAsync()
    {
        if (!IsDeletePending || SelectedRow is not { } row)
        {
            return;
        }

        IsDeletePending = false;
        IsBusy = true;
        var result = await PlanningCalls.WriteAsync(() => _sender.Send(new DeletePlanningSkillCommand(row.Skill.Id)), _format, _localization);
        if (result.IsSuccess)
        {
            StatusMessage = _localization["ResourcePlanning_Taxonomy_Deleted"];
            SelectedRow = null;
            Clear();
            await ReloadAfterWriteAsync(null);
        }
        else
        {
            StatusMessage = result.Message;
        }

        IsBusy = false;
    }

    partial void OnSelectedRowChanged(SkillTaxonomyRow? value)
    {
        OnPropertyChanged(nameof(IsEditing));
        IsDeletePending = false;
        if (value is null)
        {
            return;
        }

        CategoryText = value.Skill.Category;
        SkillNameText = value.Skill.SkillName;
        RequiresCertification = value.Skill.RequiresCertification;
        CertificationCodeText = value.Skill.CertificationCode ?? string.Empty;
    }

    private void Clear()
    {
        CategoryText = string.Empty;
        SkillNameText = string.Empty;
        RequiresCertification = false;
        CertificationCodeText = string.Empty;
    }

    private async Task ReloadAfterWriteAsync(Guid? selectId)
    {
        var failure = await PlanningCalls.ReadAsync(() => ReloadAsync(selectId), _localization);
        StatusMessage = failure ?? StatusMessage;
    }

    private async Task ReloadAsync(Guid? selectId)
    {
        var skills = await _sender.Send(new GetPlanningSkillsQuery(null));

        Rows.Clear();
        foreach (var skill in skills.OrderBy(item => item.Category, StringComparer.CurrentCultureIgnoreCase).ThenBy(item => item.SkillName, StringComparer.CurrentCultureIgnoreCase))
        {
            Rows.Add(new SkillTaxonomyRow(skill, ResourcePlanningFormatter.OrDash(skill.RequiresCertification ? skill.CertificationCode : null)));
        }

        HasData = Rows.Count > 0;
        SelectedRow = selectId is { } id ? Rows.FirstOrDefault(row => row.Skill.Id == id) : null;
        if (!HasData)
        {
            StatusMessage = _localization["ResourcePlanning_Taxonomy_Empty"];
        }
    }
}
