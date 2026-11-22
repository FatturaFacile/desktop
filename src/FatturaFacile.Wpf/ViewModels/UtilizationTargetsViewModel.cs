// <copyright file="UtilizationTargetsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.ResourcePlanning;
using FatturaFacile.Application.ResourcePlanning;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Edits the billable utilization targets: one for the whole tenant, and one for each department or cost center that has its own.
/// Setting a target of a scope replaces the previous one.
/// </summary>
internal sealed partial class UtilizationTargetsViewModel : ObservableObject
{
    private const string DepartmentPrefix = "D:";
    private const string CostCenterPrefix = "C:";
    private const string PercentFormat = "0.##";

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly ResourcePlanningFormatter _format;
    private readonly Dictionary<string, string> _scopeNames = new(StringComparer.Ordinal);

    [ObservableProperty]
    private PlanningChoice? _selectedScope;

    [ObservableProperty]
    private string _targetPercentText = string.Empty;

    [ObservableProperty]
    private UtilizationTargetRow? _selectedRow;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="UtilizationTargetsViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    public UtilizationTargetsViewModel(ISender sender, ILocalizationService localization)
    {
        _sender = sender;
        _localization = localization;
        _format = new ResourcePlanningFormatter(localization);
        ScopeChoices = [new PlanningChoice(string.Empty, _localization["ResourcePlanning_Targets_Default"])];
        _selectedScope = ScopeChoices[0];
    }

    /// <summary>
    /// Gets the scopes a target can be set for: the default of the tenant, the departments and the cost centers.
    /// </summary>
    public ObservableCollection<PlanningChoice> ScopeChoices { get; }

    /// <summary>
    /// Gets the targets that are set.
    /// </summary>
    public ObservableCollection<UtilizationTargetRow> Rows { get; } = [];

    /// <summary>
    /// Loads the targets and the departments and cost centers they can be set for.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        var failure = await PlanningCalls.ReadAsync(ReloadAsync, _localization);
        StatusMessage = failure ?? StatusMessage;
        IsBusy = false;
    }

    /// <summary>
    /// Sets the target of the scope of the form.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand]
    public async Task SaveAsync()
    {
        var key = SelectedScope?.Key ?? string.Empty;
        var built = PlanningDraftBuilder.Target(IdOf(key, DepartmentPrefix), IdOf(key, CostCenterPrefix), (double?)_format.ParseDecimal(TargetPercentText));
        if (built.Request is not { } request)
        {
            StatusMessage = _format.Describe("Setup", built.ErrorCode ?? string.Empty);

            return;
        }

        IsBusy = true;
        var result = await PlanningCalls.WriteAsync(() => _sender.Send(new SetUtilizationTargetCommand(request)), _format, _localization);
        if (result.IsSuccess)
        {
            StatusMessage = _localization["ResourcePlanning_Targets_Saved"];
            var failure = await PlanningCalls.ReadAsync(ReloadAsync, _localization);
            StatusMessage = failure ?? StatusMessage;
        }
        else
        {
            StatusMessage = result.Message;
        }

        IsBusy = false;
    }

    private static Guid? IdOf(string key, string prefix) =>
        key.StartsWith(prefix, StringComparison.Ordinal) && Guid.TryParse(key.AsSpan(prefix.Length), out var id) ? id : null;

    private static string KeyOf(UtilizationTarget target) =>
        target.DepartmentId is { } department
            ? DepartmentPrefix + department.ToString("D", CultureInfo.InvariantCulture)
            : target.CostCenterId is { } costCenter ? CostCenterPrefix + costCenter.ToString("D", CultureInfo.InvariantCulture) : string.Empty;

    partial void OnSelectedRowChanged(UtilizationTargetRow? value)
    {
        if (value is null)
        {
            return;
        }

        SelectedScope = ScopeChoices.FirstOrDefault(choice => string.Equals(choice.Key, value.ScopeKey, StringComparison.Ordinal)) ?? ScopeChoices[0];
        TargetPercentText = value.Target.TargetPercent.ToString(PercentFormat, _format.Culture);
    }

    private async Task ReloadAsync()
    {
        await LoadScopesAsync();
        var targets = await _sender.Send(new GetUtilizationTargetsQuery());
        var selectedKey = SelectedRow?.ScopeKey;

        Rows.Clear();
        foreach (var target in targets.OrderBy(item => ScopeTextOf(KeyOf(item)), StringComparer.CurrentCultureIgnoreCase))
        {
            var key = KeyOf(target);
            Rows.Add(new UtilizationTargetRow(target, key, ScopeTextOf(key), _format.Percent(target.TargetPercent)));
        }

        HasData = Rows.Count > 0;
        SelectedRow = selectedKey is null ? null : Rows.FirstOrDefault(row => string.Equals(row.ScopeKey, selectedKey, StringComparison.Ordinal));
        if (!HasData)
        {
            StatusMessage = _localization["ResourcePlanning_Targets_Empty"];
        }
    }

    private string ScopeTextOf(string key) =>
        key.Length == 0
            ? _localization["ResourcePlanning_Targets_Default"]
            : _scopeNames.TryGetValue(key, out var name) ? name : key[DepartmentPrefix.Length..];

    private async Task LoadScopesAsync()
    {
        if (ScopeChoices.Count > 1)
        {
            return;
        }

        var resources = await _sender.Send(new GetPlanningResourcesQuery(null));
        AddScopes(
            resources.Where(resource => resource.DepartmentId is not null).GroupBy(resource => resource.DepartmentId!.Value).Select(group => (Id: group.Key, Name: ResourcePlanningFormatter.OrDash(group.First().DepartmentName))),
            DepartmentPrefix,
            "ResourcePlanning_Targets_Department");
        AddScopes(
            resources.Where(resource => resource.CostCenterId is not null).GroupBy(resource => resource.CostCenterId!.Value).Select(group => (Id: group.Key, Name: ResourcePlanningFormatter.OrDash(group.First().CostCenterName))),
            CostCenterPrefix,
            "ResourcePlanning_Targets_CostCenter");
    }

    private void AddScopes(IEnumerable<(Guid Id, string Name)> scopes, string prefix, string labelKey)
    {
        foreach (var scope in scopes.OrderBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase))
        {
            var key = prefix + scope.Id.ToString("D", CultureInfo.InvariantCulture);
            var label = string.Format(_format.Culture, _localization[labelKey], scope.Name);
            _scopeNames[key] = label;
            ScopeChoices.Add(new PlanningChoice(key, label));
        }
    }
}
