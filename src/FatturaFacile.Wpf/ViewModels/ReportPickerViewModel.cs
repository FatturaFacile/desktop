// <copyright file="ReportPickerViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Application.Features.Reports.Commands;
using FatturaFacile.Application.Features.Reports.Queries;
using FatturaFacile.Wpf.Services;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Lists the templates of a document type found on the file system and lets the user choose, duplicate or edit them.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ReportPickerViewModel"/> class.
/// </remarks>
/// <param name="sender">The MediatR sender.</param>
/// <param name="templateStore">The template store.</param>
/// <param name="shellLauncher">The shell launcher.</param>
/// <param name="localizationService">The localization service.</param>
/// <param name="documentType">The document type.</param>
internal sealed partial class ReportPickerViewModel(ISender sender, IReportTemplateStore templateStore, IShellLauncher shellLauncher, ILocalizationService localizationService, ReportDocumentType documentType) : ObservableObject
{
    private readonly ISender _sender = sender;
    private readonly IReportTemplateStore _templateStore = templateStore;
    private readonly IShellLauncher _shellLauncher = shellLauncher;
    private readonly ILocalizationService _localizationService = localizationService;

    [ObservableProperty]
    private ReportTemplateInfo? _selectedTemplate;

    [ObservableProperty]
    private string _newTemplateName = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool? _dialogResult;

    /// <summary>
    /// Gets the document type.
    /// </summary>
    public ReportDocumentType DocumentType { get; } = documentType;

    /// <summary>
    /// Gets the window title.
    /// </summary>
    public string Title => _localizationService["Report_Picker_Title"] + " - " + _localizationService["Report_Type_" + DocumentType.ToString()];

    /// <summary>
    /// Gets the templates.
    /// </summary>
    public ObservableCollection<ReportTemplateInfo> Templates { get; } = [];

    /// <summary>
    /// Gets the template folder of the document type.
    /// </summary>
    public string TemplateFolder => _templateStore.GetDirectory(DocumentType);

    /// <summary>
    /// Reloads the templates from the folder (templates added or renamed in Explorer appear immediately).
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadTemplatesAsync()
    {
        try
        {
            var previous = SelectedTemplate?.Name;
            var templates = await _sender.Send(new GetReportTemplatesQuery(DocumentType));
            Templates.Clear();

            foreach (var template in templates)
            {
                Templates.Add(template);
            }

            SelectedTemplate = Templates.FirstOrDefault(t => string.Equals(t.Name, previous, StringComparison.OrdinalIgnoreCase))
                ?? PreferredTemplate();
            StatusMessage = string.Format(CultureInfo.CurrentCulture, _localizationService["Report_TemplatesFound"], Templates.Count);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void Confirm()
    {
        if (SelectedTemplate is not null)
        {
            DialogResult = true;
        }
    }

    [RelayCommand]
    private void Cancel() => DialogResult = false;

    [RelayCommand]
    private async Task DuplicateAsync()
    {
        if (SelectedTemplate is null)
        {
            return;
        }

        try
        {
            var name = string.IsNullOrWhiteSpace(NewTemplateName) ? SelectedTemplate.Name + "_copia" : NewTemplateName.Trim();
            var copy = await _sender.Send(new DuplicateReportTemplateCommand(DocumentType, SelectedTemplate.Name, name));
            NewTemplateName = string.Empty;
            await LoadTemplatesAsync();
            SelectedTemplate = Templates.FirstOrDefault(t => string.Equals(t.Name, copy.Name, StringComparison.OrdinalIgnoreCase));
            StatusMessage = string.Format(CultureInfo.CurrentCulture, _localizationService["Report_Duplicated"], copy.Name);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void EditTemplate()
    {
        if (SelectedTemplate is not null)
        {
            _shellLauncher.Open(SelectedTemplate.FilePath);
        }
    }

    [RelayCommand]
    private void OpenFolder() => _shellLauncher.Open(TemplateFolder);

    private ReportTemplateInfo? PreferredTemplate()
    {
        var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToUpperInvariant();
        return Templates.FirstOrDefault(t => t.Name.EndsWith("_" + language, StringComparison.OrdinalIgnoreCase)) ?? Templates.FirstOrDefault();
    }
}
