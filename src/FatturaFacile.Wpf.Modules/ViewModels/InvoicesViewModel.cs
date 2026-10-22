// <copyright file="InvoicesViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Invoices;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.ValueObjects;
using MediatR;

namespace FatturaFacile.ViewModels;

/// <summary>
/// Represents invoices view model.
/// </summary>
/// <param name="mediator">The mediator.</param>
/// <param name="invoiceDocumentService">The invoice document generator service.</param>
public sealed partial class InvoicesViewModel(ISender mediator, IInvoiceDocumentService? invoiceDocumentService = null) : ViewModelBase
{
    private readonly ISender mediator = mediator;
    private readonly IInvoiceDocumentService? invoiceDocumentService = invoiceDocumentService;

    /// <summary>
    /// Gets or sets the invoices.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<ItalianInvoiceDto> Invoices { get; set; } = [];

    /// <summary>
    /// Gets or sets the swiss qr bills.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<SwissQrBillDto> SwissQrBills { get; set; } = [];

    /// <summary>
    /// Gets or sets the selected invoice.
    /// </summary>
    [ObservableProperty]
    public partial ItalianInvoiceDto? SelectedInvoice { get; set; }

    /// <summary>
    /// Gets or sets the selected swiss qr bill.
    /// </summary>
    [ObservableProperty]
    public partial SwissQrBillDto? SelectedSwissQrBill { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether preview open.
    /// </summary>
    [ObservableProperty]
    public partial bool IsPreviewOpen { get; set; }

    /// <summary>
    /// Gets or sets the preview xml.
    /// </summary>
    [ObservableProperty]
    public partial string PreviewXml { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the preview svg.
    /// </summary>
    [ObservableProperty]
    public partial string PreviewSvg { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether swiss tab selected.
    /// </summary>
    [ObservableProperty]
    public partial bool IsSwissTabSelected { get; set; }

    /// <summary>
    /// Initialize Async.
    /// </summary>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public override Task InitializeAsync() => LoadDataAsync();

    /// <summary>
    /// Load Data Async.
    /// </summary>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var overview = await mediator.Send(new GetInvoicesOverviewQuery());
            Invoices = [..overview.ItalianInvoices];
            SwissQrBills = [..overview.SwissQrBills];

            SelectedInvoice = Invoices.FirstOrDefault();
            SelectedSwissQrBill = SwissQrBills.FirstOrDefault();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// View Fattura Pa Xml.
    /// </summary>
    /// <param name="invoice">The invoice.</param>
    [RelayCommand]
    public void ViewFatturaPaXml(
        ItalianInvoiceDto invoice)
    {
        if (invoice == null)
        {
            return;
        }

        SelectedInvoice = invoice;

        // Costruisce l'aggregato per generare l'XML nativo
        var dummyInvoice = ItalianElectronicInvoice.Create(
            invoice.TenantId,
            invoice.DocumentNumber,
            invoice.DocumentDate,
            invoice.ClientId,
            invoice.CedenteDenominazione,
            invoice.CedentePartitaIva,
            invoice.CedentePartitaIva,
            new Address(
                "Via Monte Napoleone 8",
                "Milano",
                "20121",
                "MI",
                "IT"),
            invoice.CessionarioDenominazione,
            invoice.CessionarioPartitaIva,
            cessionarioCf: null,
            invoice.CessionarioCodiceDestinatario,
            pec: null,
            new Address(
                "Via delle Industrie 45",
                "Monza",
                "20900",
                "MB",
                "IT"),
            invoice.PaymentDueDate);

        foreach (var line in invoice.Lines)
        {
            dummyInvoice.AddLine(
                line.Description,
                line.Quantity,
                line.UnitPrice,
                line.VatRate,
                line.NaturaExemptionCode);
        }

        PreviewXml = invoiceDocumentService?.GenerateFatturaPaXml(dummyInvoice) ?? string.Empty;
        PreviewSvg = string.Empty;
        IsPreviewOpen = true;
    }

    /// <summary>
    /// View Swiss Qr Slip.
    /// </summary>
    /// <param name="bill">The bill.</param>
    [RelayCommand]
    public void ViewSwissQrSlip(
        SwissQrBillDto bill)
    {
        if (bill == null)
        {
            return;
        }

        SelectedSwissQrBill = bill;

        var dummyBill = SwissQrBill.Create(
            bill.TenantId,
            bill.BillNumber,
            bill.BillDate,
            bill.ClientId,
            bill.CreditorName,
            new Address(
                "Via Nassa 12",
                "Lugano",
                "6900",
                "TI",
                "CH"),
            new QrIban(bill.CreditorQrIban),
            bill.DebtorName,
            new Address(
                "Via San Gottardo 102",
                "Bellinzona",
                "6500",
                "TI",
                "CH"),
            Money.Chf(bill.Amount),
            bill.ReferenceNumber,
            bill.DueDate,
            bill.AdditionalMessage);

        PreviewSvg = invoiceDocumentService?.GenerateSwissQrSvg(dummyBill) ?? string.Empty;
        PreviewXml = string.Empty;
        IsPreviewOpen = true;
    }

    /// <summary>
    /// Close Preview.
    /// </summary>
    [RelayCommand]
    public void ClosePreview()
    {
        IsPreviewOpen = false;
        PreviewXml = string.Empty;
        PreviewSvg = string.Empty;
    }
}
