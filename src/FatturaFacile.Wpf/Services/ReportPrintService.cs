// <copyright file="ReportPrintService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Application.Features.Reports.Queries;
using FatturaFacile.Wpf.ViewModels;
using FatturaFacile.Wpf.Views;
using MediatR;

namespace FatturaFacile.Wpf.Services;

/// <summary>
/// Shows the template picker and the PDF preview windows.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ReportPrintService"/> class.
/// </remarks>
/// <param name="sender">The MediatR sender.</param>
/// <param name="templateStore">The template store.</param>
/// <param name="shellLauncher">The shell launcher.</param>
/// <param name="fileDialogService">The file dialogs.</param>
/// <param name="localizationService">The localization service.</param>
internal sealed class ReportPrintService(
    ISender sender,
    IReportTemplateStore templateStore,
    IShellLauncher shellLauncher,
    IFileDialogService fileDialogService,
    ILocalizationService localizationService) : IReportPrintService
{
    /// <inheritdoc/>
    public Task<bool> PrintAsync(ReportDocumentType documentType, Guid entityId) => PrintInternalAsync(documentType, entityId, from: null, to: null);

    /// <inheritdoc/>
    public Task<bool> PrintAsync(ReportDocumentType documentType, Guid entityId, DateTime from, DateTime to) => PrintInternalAsync(documentType, entityId, from, to);

    private async Task<bool> PrintInternalAsync(ReportDocumentType documentType, Guid entityId, DateTime? from, DateTime? to)
    {
        ReportPickerViewModel picker = new(sender, templateStore, shellLauncher, localizationService, documentType);
        await picker.LoadTemplatesAsync();

        ReportPickerWindow pickerWindow = new() { DataContext = picker, Owner = System.Windows.Application.Current?.MainWindow };

        if (pickerWindow.ShowDialog() != true || picker.SelectedTemplate is null)
        {
            return false;
        }

        var report = await sender.Send(new RenderReportQuery(documentType, picker.SelectedTemplate.Name, entityId, from, to));
        ReportPreviewViewModel preview = new(sender, fileDialogService, shellLauncher, localizationService, report);
        ReportPreviewWindow previewWindow = new() { DataContext = preview, Owner = System.Windows.Application.Current?.MainWindow };
        previewWindow.Closed += (_, _) => preview.Dispose();
        previewWindow.Show();
        return true;
    }
}
