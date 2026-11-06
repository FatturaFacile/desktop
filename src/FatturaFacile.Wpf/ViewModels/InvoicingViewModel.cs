// <copyright file="InvoicingViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Invoicing.Commands;
using FatturaFacile.Application.Features.Invoicing.Queries;
using FatturaFacile.Domain.Common;
using FatturaFacile.Wpf.Services;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// View model governing invoicing operations, issue workflows, and document exports.
/// </summary>
internal sealed partial class InvoicingViewModel : ObservableObject
{
    private readonly ISender _sender;
    private readonly IDocumentExportService _exportService;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ILocalizationService _localizationService;
    private readonly IReportPrintService? _printService;

    [ObservableProperty]
    private ObservableCollection<InvoiceDto> _invoices = [];

    [ObservableProperty]
    private InvoiceDto? _selectedInvoice;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage;

    [ObservableProperty]
    private string _countryCode;

    [ObservableProperty]
    private string _currencyCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoicingViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender instance.</param>
    /// <param name="exportService">Syncfusion document export service.</param>
    /// <param name="currentTenantService">The current tenant context service.</param>
    /// <param name="localizationService">The localization service.</param>
    public InvoicingViewModel(
        ISender sender,
        IDocumentExportService exportService,
        ICurrentTenantService currentTenantService,
        ILocalizationService localizationService)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(currentTenantService);
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(exportService);

        _sender = sender;
        _exportService = exportService;
        _currentTenantService = currentTenantService;
        _localizationService = localizationService;

        _countryCode = currentTenantService.CountryCode;
        _currencyCode = currentTenantService.DefaultCurrency;
        _statusMessage = localizationService["Common_Ready"];

        _currentTenantService.CurrentTenantChanged += (_, _) => _ = OnTenantChangedAsync();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoicingViewModel"/> class with printing through templates.
    /// </summary>
    /// <param name="sender">The MediatR sender instance.</param>
    /// <param name="exportService">Syncfusion document export service.</param>
    /// <param name="currentTenantService">The current tenant context service.</param>
    /// <param name="localizationService">The localization service.</param>
    /// <param name="printService">The template print service.</param>
    public InvoicingViewModel(
        ISender sender,
        IDocumentExportService exportService,
        ICurrentTenantService currentTenantService,
        ILocalizationService localizationService,
        IReportPrintService printService)
        : this(sender, exportService, currentTenantService, localizationService)
    {
        _printService = printService;
    }

    /// <summary>
    /// Gets a value indicating whether the current tenant jurisdiction is Swiss (ISO 20022 QR-Bill).
    /// </summary>
    public bool IsSwiss => string.Equals(CountryCode, "CH", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether the current tenant jurisdiction is Italian (SDI FatturaPA).
    /// </summary>
    public bool IsItalian => !IsSwiss;

    /// <summary>
    /// Loads invoices for current tenant context.
    /// </summary>
    /// <returns>A task representing the load operation.</returns>
    [RelayCommand]
    public async Task LoadInvoicesAsync()
    {
        IsLoading = true;
        StatusMessage = _localizationService["Common_Loading"];
        try
        {
            CountryCode = _currentTenantService.CountryCode;
            CurrencyCode = _currentTenantService.DefaultCurrency;

            var invoices = await _sender.Send(new GetInvoicesQuery());
            Invoices.Clear();
            foreach (var inv in invoices)
            {
                Invoices.Add(inv);
            }

            SelectedInvoice = Invoices.FirstOrDefault();
            StatusMessage = string.Format(_localizationService.CurrentCulture, _localizationService["Invoicing_LoadedCount"], Invoices.Count);
        }
        catch (Exception ex)
        {
            StatusMessage = $"{_localizationService["Common_Error"]}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Issues the currently selected invoice, generating QR-Bill or SDI metadata based on country.
    /// </summary>
    /// <returns>A task representing the issue operation.</returns>
    [RelayCommand]
    public async Task IssueInvoiceAsync()
    {
        if (SelectedInvoice == null)
        {
            StatusMessage = _localizationService["Invoicing_NoSelection"];
            return;
        }

        IsLoading = true;
        try
        {
            var success = await _sender.Send(new IssueInvoiceCommand(SelectedInvoice.Id));
            if (success)
            {
                await LoadInvoicesAsync();
                StatusMessage = _localizationService["Common_Success"];
            }
            else
            {
                StatusMessage = _localizationService["Common_Error"];
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"{_localizationService["Common_Error"]}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Prints the selected invoice with a template chosen by the user (Word templates on the file system).
    /// </summary>
    /// <returns>A task representing the print.</returns>
    [RelayCommand]
    public async Task PrintAsync()
    {
        if (SelectedInvoice is null || _printService is null)
        {
            StatusMessage = _localizationService["Invoicing_NoSelection"];
            return;
        }

        try
        {
            _ = await _printService.PrintAsync(ReportDocumentType.Invoice, SelectedInvoice.Id);
        }
        catch (Exception ex)
        {
            StatusMessage = $"{_localizationService["Common_Error"]}: {ex.Message}";
        }
    }

    /// <summary>
    /// Exports the selected invoice to a PDF document via non-visual Syncfusion engine.
    /// </summary>
    /// <returns>A task representing PDF generation.</returns>
    [RelayCommand]
    public async Task ExportPdfAsync()
    {
        if (SelectedInvoice == null)
        {
            StatusMessage = _localizationService["Invoicing_NoSelection"];
            return;
        }

        IsLoading = true;
        try
        {
            var pdfBytes = await _exportService.GenerateInvoicePdfAsync(SelectedInvoice);
            var tempPath = Path.Combine(Path.GetTempPath(), $"Fattura_{SelectedInvoice.InvoiceNumber.Replace('/', '_')}.pdf");
            await File.WriteAllBytesAsync(tempPath, pdfBytes);
            StatusMessage = $"{_localizationService["Common_Success"]}: {tempPath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"{_localizationService["Common_Error"]}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Exports the entire list of invoices to Excel spreadsheet via Syncfusion XlsIO.
    /// </summary>
    /// <returns>A task representing Excel export.</returns>
    [RelayCommand]
    public async Task ExportExcelAsync()
    {
        if (Invoices.Count == 0)
        {
            StatusMessage = _localizationService["Invoicing_NoSelection"];
            return;
        }

        IsLoading = true;
        try
        {
            var xlsBytes = await _exportService.ExportInvoicesToExcelAsync(Invoices);
            var tempPath = Path.Combine(Path.GetTempPath(), string.Create(CultureInfo.InvariantCulture, $"Fatture_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx"));
            await File.WriteAllBytesAsync(tempPath, xlsBytes);
            StatusMessage = $"{_localizationService["Common_Success"]}: {tempPath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"{_localizationService["Common_Error"]}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnCountryCodeChanged(string value)
    {
        OnPropertyChanged(nameof(IsSwiss));
        OnPropertyChanged(nameof(IsItalian));
    }

    private Task OnTenantChangedAsync()
    {
        SelectedInvoice = null;
        Invoices.Clear();
        CountryCode = _currentTenantService.CountryCode;
        CurrencyCode = _currentTenantService.DefaultCurrency;
        return LoadInvoicesAsync();
    }
}
