using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Invoices;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.ValueObjects;
using FatturaFacile.Infrastructure.CrossBorder.FatturaPa;
using FatturaFacile.Infrastructure.CrossBorder.SwissQr;
using MediatR;

namespace FatturaFacile.ViewModels;

public sealed partial class InvoicesViewModel(ISender mediator) : ViewModelBase
{
    private readonly ISender _mediator = mediator;

    [ObservableProperty]
    public partial ObservableCollection<ItalianInvoiceDto> ItalianInvoices { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<SwissQrBillDto> SwissQrBills { get; set; } = [];

    [ObservableProperty]
    public partial ItalianInvoiceDto? SelectedItalianInvoice { get; set; }

    [ObservableProperty]
    public partial SwissQrBillDto? SelectedSwissQrBill { get; set; }

    [ObservableProperty]
    public partial string PreviewXml { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PreviewSvg { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsPreviewOpen { get; set; }

    public override Task InitializeAsync() => LoadInvoicesAsync();

    [RelayCommand]
    public async Task LoadInvoicesAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var overview = await _mediator.Send(new GetInvoicesOverviewQuery());
            ItalianInvoices = [with(overview.ItalianInvoices)];
            SwissQrBills = [with(overview.SwissQrBills)];
            SelectedItalianInvoice = ItalianInvoices.FirstOrDefault();
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

    [RelayCommand]
    public void ViewItalianInvoiceXml(ItalianInvoiceDto invoice)
    {
        if (invoice == null)
        {
            return;
        }

        SelectedItalianInvoice = invoice;

        var dummyInvoice = ItalianElectronicInvoice.Create(
            invoice.TenantId,
            invoice.DocumentNumber,
            invoice.DocumentDate,
            invoice.ClientId,
            invoice.CedenteDenominazione,
            invoice.CedentePartitaIva,
            invoice.CedentePartitaIva,
            new Address("Via Monte Napoleone 8", "Milano", "20121", "MI", "IT"),
            invoice.CessionarioDenominazione,
            invoice.CessionarioPartitaIva,
cessionarioCf: null,
            invoice.CessionarioCodiceDestinatario,
pec: null,
            new Address("Via delle Industrie 45", "Monza", "20900", "MB", "IT"),
            invoice.PaymentDueDate);

        foreach (var line in invoice.Lines)
        {
            dummyInvoice.AddLine(line.Description, line.Quantity, line.UnitPrice, line.VatRate, line.NaturaExemptionCode);
        }

        PreviewXml = FatturaPaXmlGenerator.GenerateXml(dummyInvoice);
        PreviewSvg = string.Empty;
        IsPreviewOpen = true;
    }

    [RelayCommand]
    public void ViewSwissQrSlip(SwissQrBillDto bill)
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
            new Address("Via Nassa 12", "Lugano", "6900", "TI", "CH"),
            new QrIban(bill.CreditorQrIban),
            bill.DebtorName,
            new Address("Via San Gottardo 102", "Bellinzona", "6500", "TI", "CH"),
            Money.Chf(bill.Amount),
            bill.ReferenceNumber,
            bill.DueDate,
            bill.AdditionalMessage);

        PreviewSvg = SwissQrBillGenerator.GenerateSvgPreview(dummyBill);
        PreviewXml = string.Empty;
        IsPreviewOpen = true;
    }

    [RelayCommand]
    public void ClosePreview() => IsPreviewOpen = false;
}
