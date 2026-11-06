// <copyright file="ReportPreviewViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Application.Features.Reports.Commands;
using FatturaFacile.Wpf.Services;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Shows a generated PDF in the Syncfusion PDF viewer (print, zoom, search) and saves it in the archive or elsewhere.
/// </summary>
internal sealed partial class ReportPreviewViewModel : ObservableObject, IDisposable
{
    private readonly ISender _sender;
    private readonly IFileDialogService _fileDialogService;
    private readonly IShellLauncher _shellLauncher;
    private readonly ILocalizationService _localizationService;
    private readonly RenderedReport _report;
    private string? _archivedPath;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportPreviewViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="fileDialogService">The file dialogs.</param>
    /// <param name="shellLauncher">The shell launcher.</param>
    /// <param name="localizationService">The localization service.</param>
    /// <param name="report">The generated report.</param>
    public ReportPreviewViewModel(
        ISender sender,
        IFileDialogService fileDialogService,
        IShellLauncher shellLauncher,
        ILocalizationService localizationService,
        RenderedReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        _sender = sender;
        _fileDialogService = fileDialogService;
        _shellLauncher = shellLauncher;
        _localizationService = localizationService;
        _report = report;
        Document = new MemoryStream(report.Content.ToArray(), writable: false);
    }

    /// <summary>
    /// Gets the PDF stream shown by the viewer.
    /// </summary>
    public MemoryStream Document { get; }

    /// <summary>
    /// Gets the window title.
    /// </summary>
    public string Title => _report.FileName;

    /// <inheritdoc/>
    public void Dispose() => Document.Dispose();

    [RelayCommand]
    private async Task ArchiveAsync()
    {
        try
        {
            _archivedPath = await _sender.Send(new ArchiveReportCommand(_report));
            StatusMessage = string.Format(CultureInfo.CurrentCulture, _localizationService["Report_Archived"], _archivedPath);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void OpenArchiveFolder()
    {
        if (_archivedPath is not null && Path.GetDirectoryName(_archivedPath) is { } folder)
        {
            _shellLauncher.Open(folder);
        }
    }

    [RelayCommand]
    private async Task SaveAsAsync()
    {
        var path = _fileDialogService.ShowSaveFile(_localizationService["Report_SaveAs"], "PDF (*.pdf)|*.pdf", _report.FileName);

        if (path is null)
        {
            return;
        }

        try
        {
            await File.WriteAllBytesAsync(path, _report.Content.ToArray());
            StatusMessage = string.Format(CultureInfo.CurrentCulture, _localizationService["Report_Saved"], path);
        }
        catch (IOException ex)
        {
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task OpenExternalAsync()
    {
        var path = Path.Combine(Path.GetTempPath(), _report.FileName);
        await File.WriteAllBytesAsync(path, _report.Content.ToArray());
        _shellLauncher.Open(path);
    }
}
