// <copyright file="PurchasesSectionViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Application.Features.Purchasing;
using FatturaFacile.Application.Features.Records;
using FatturaFacile.Application.Purchasing;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Wpf.Services;
using FatturaFacile.Wpf.ViewModels.Payroll;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels.Finance;

/// <summary>
/// Supplier invoices (ciclo passivo): registration with lines, VAT deduction, posting.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PurchasesSectionViewModel"/> class.
/// </remarks>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localizationService">The localization service.</param>
/// <param name="fileDialogService">The file dialogs.</param>
internal sealed partial class PurchasesSectionViewModel(ISender sender, ILocalizationService localizationService, IFileDialogService fileDialogService) : PayrollSectionViewModel(sender, localizationService)
{
    [ObservableProperty]
    private PurchaseInvoice? _selectedInvoice;

    [ObservableProperty]
    private PurchaseInvoice _editing = NewInvoice();

    [ObservableProperty]
    private PurchaseInvoiceLine _newLine = NewLineTemplate();

    [ObservableProperty]
    private PurchaseInvoiceLine? _selectedLine;

    /// <summary>
    /// Gets the supplier invoices.
    /// </summary>
    public ObservableCollection<PurchaseInvoice> Invoices { get; } = [];

    /// <summary>
    /// Gets the lines of the edited invoice.
    /// </summary>
    public ObservableCollection<PurchaseInvoiceLine> Lines { get; } = [];

    /// <summary>
    /// Gets the suppliers.
    /// </summary>
    public ObservableCollection<Contact> Suppliers { get; } = [];

    /// <summary>
    /// Gets the accounts.
    /// </summary>
    public ObservableCollection<ChartOfAccount> Accounts { get; } = [];

    /// <summary>
    /// Gets the totals of the edited invoice.
    /// </summary>
    public string Totals
    {
        get
        {
            PurchaseInvoice preview = new();
            PurchaseInvoiceCalculator.Compute(preview, [.. Lines.Select(RecordCopier.Clone)]);
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, Localization["Finance_PurchaseTotals"], preview.TotalNet, preview.TotalVat, preview.DeductibleVat, preview.TotalGross);
        }
    }

    /// <inheritdoc/>
    [RelayCommand]
    public override Task LoadAsync() => RunAsync(async () =>
    {
        var selectedId = SelectedInvoice?.Id;
        var contacts = await Sender.Send(new GetRecordsQuery<Contact>());
        var accounts = await Sender.Send(new GetRecordsQuery<ChartOfAccount>());
        var invoices = await Sender.Send(new GetRecordsQuery<PurchaseInvoice>());
        Replace(Suppliers, contacts.Where(static c => !string.Equals(c.Type, "client", StringComparison.OrdinalIgnoreCase)).OrderBy(static c => c.DisplayName, StringComparer.CurrentCultureIgnoreCase));
        Replace(Accounts, accounts.OrderBy(static a => a.Code ?? a.AccountCode, StringComparer.Ordinal));
        Replace(Invoices, invoices.OrderByDescending(static i => i.RegistrationDate));
        SelectedInvoice = Invoices.FirstOrDefault(i => i.Id == selectedId) ?? Invoices.FirstOrDefault();
    });

    private static PurchaseInvoice NewInvoice() => new() { Id = Guid.Empty, Currency = "CHF" };

    private static PurchaseInvoiceLine NewLineTemplate() => new() { VatRate = 8.1m, DeductiblePercent = 100m };

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();

        foreach (var item in items)
        {
            target.Add(item);
        }
    }

    partial void OnSelectedInvoiceChanged(PurchaseInvoice? value)
    {
        Editing = value is null ? NewInvoice() : RecordCopier.Clone(value);
        _ = LoadLinesAsync();
    }

    [RelayCommand]
    private void New()
    {
        SelectedInvoice = null;
        Editing = NewInvoice();
        Lines.Clear();
        OnPropertyChanged(nameof(Totals));
    }

    [RelayCommand]
    private void AddLine()
    {
        Lines.Add(RecordCopier.Clone(NewLine));
        NewLine = NewLineTemplate();
        OnPropertyChanged(nameof(Totals));
    }

    [RelayCommand]
    private void RemoveLine()
    {
        if (SelectedLine is not null)
        {
            _ = Lines.Remove(SelectedLine);
            OnPropertyChanged(nameof(Totals));
        }
    }

    [RelayCommand]
    private Task SaveAsync() => RunAsync(
        async () =>
        {
            var id = await Sender.Send(new SavePurchaseInvoiceCommand(Editing, [.. Lines]));
            await LoadAsync();
            SelectedInvoice = Invoices.FirstOrDefault(i => i.Id == id);
        },
        "Common_Saved");

    [RelayCommand]
    private Task PostAsync() => SelectedInvoice is null
        ? Task.CompletedTask
        : RunAsync(
            async () =>
            {
                _ = await Sender.Send(new PostPurchaseInvoiceCommand(SelectedInvoice.Id));
                await LoadAsync();
            },
            "Finance_Posted");

    [RelayCommand]
    private Task DeleteAsync() => SelectedInvoice is null
        ? Task.CompletedTask
        : RunAsync(
            async () =>
            {
                _ = await Sender.Send(new DeleteRecordCommand<PurchaseInvoice>(SelectedInvoice.Id));
                await LoadAsync();
            },
            "Common_Deleted");

    private async Task LoadLinesAsync()
    {
        Lines.Clear();

        if (SelectedInvoice is { } invoice)
        {
            var invoiceId = invoice.Id;
            var lines = await Sender.Send(new GetRecordsQuery<PurchaseInvoiceLine>(l => l.PurchaseInvoiceId == invoiceId));

            foreach (var line in lines)
            {
                Lines.Add(line);
            }
        }

        OnPropertyChanged(nameof(Totals));
    }

    [RelayCommand]
    private Task ImportXmlAsync()
    {
        var path = fileDialogService.ShowOpenFile(Localization["Finance_ImportXml"], "FatturaPA (*.xml;*.p7m)|*.xml;*.p7m");

        return path is null
            ? Task.CompletedTask
            : RunAsync(async () =>
            {
                var result = await Sender.Send(new ImportPurchaseInvoiceFileCommand(Path.GetFileName(path), await File.ReadAllBytesAsync(path)));
                await LoadAsync();
                StatusMessage = string.Format(CultureInfo.CurrentCulture, Localization["Finance_ImportXmlDone"], result.Created, result.Skipped);
            });
    }
}
