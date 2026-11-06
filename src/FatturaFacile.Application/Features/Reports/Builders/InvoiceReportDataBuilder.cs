// <copyright file="InvoiceReportDataBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Features.Reports.Builders;

/// <summary>
/// Builds the merge fields of an invoice: company, customer, header, totals, the "Items" and "VatSummary" groups
/// and, for Swiss invoices, the QR-bill payment part.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InvoiceReportDataBuilder"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class InvoiceReportDataBuilder(IUnitOfWork unitOfWork) : IReportDataBuilder
{
    /// <inheritdoc/>
    public ReportDocumentType DocumentType => ReportDocumentType.Invoice;

    /// <inheritdoc/>
    public Task<ReportData?> BuildAsync(Guid entityId, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(culture);

        return BuildInternalAsync(entityId, culture, cancellationToken);
    }

    private static void AddCompany(ReportData data, Tenant? tenant)
    {
        var country = tenant?.Country ?? "IT";
        data.Fields["Company_Name"] = tenant?.LegalName is { Length: > 0 } legal ? legal : tenant?.Name ?? string.Empty;
        data.Fields["Company_AddressLine"] = ReportFormat.StreetLine(tenant?.Street, tenant?.HouseNumber);
        data.Fields["Company_CityLine"] = ReportFormat.CityLine(tenant?.PostalCode, tenant?.City, tenant?.Province, country, country);
        data.Fields["Company_Country"] = country;
        data.Fields["Company_VatNumber"] = tenant?.VatNumber ?? string.Empty;
        data.Fields["Company_FiscalCode"] = tenant?.FiscalCode ?? string.Empty;
        data.Fields["Company_UidNumber"] = tenant?.UidNumber ?? string.Empty;
        data.Fields["Company_Iban"] = tenant?.Iban ?? string.Empty;
        data.Fields["Company_Pec"] = tenant?.PecEmail ?? string.Empty;
    }

    private static void AddCustomer(ReportData data, Contact? contact, string homeCountry)
    {
        data.Fields["Customer_Name"] = contact?.DisplayName ?? string.Empty;
        data.Fields["Customer_AddressLine"] = ReportFormat.StreetLine(contact?.Street, contact?.HouseNumber);
        data.Fields["Customer_CityLine"] = ReportFormat.CityLine(contact?.PostalCode, contact?.City, contact?.Province, contact?.Country, homeCountry);
        data.Fields["Customer_Country"] = contact?.Country ?? string.Empty;
        data.Fields["Customer_VatNumber"] = contact?.VatNumber ?? contact?.UidNumber ?? string.Empty;
        data.Fields["Customer_FiscalCode"] = contact?.FiscalCode ?? string.Empty;
        data.Fields["Customer_SdiCode"] = contact?.SdiCode ?? string.Empty;
        data.Fields["Customer_Pec"] = contact?.Pec ?? string.Empty;
        data.Fields["Customer_Email"] = contact?.Email ?? string.Empty;
    }

    private static void AddHeaderAndTotals(ReportData data, Invoice invoice, CultureInfo culture)
    {
        var language = ReportCulture.Language(culture);
        var creditNote = invoice.InvoiceType.Contains("credit", StringComparison.OrdinalIgnoreCase);
        data.Fields["Document_Title"] = InvoiceLabels.Title(language, creditNote);
        data.Fields["Invoice_Number"] = invoice.InvoiceNumber;
        data.Fields["Invoice_Date"] = ReportFormat.Date(invoice.Date, culture);
        data.Fields["Invoice_DueDate"] = ReportFormat.Date(invoice.DueDate, culture);
        data.Fields["Invoice_Currency"] = invoice.Currency;
        data.Fields["Invoice_PaymentMethod"] = invoice.PaymentMethod ?? string.Empty;
        data.Fields["Invoice_BankIban"] = invoice.BankIban ?? string.Empty;
        data.Fields["Invoice_Notes"] = invoice.Notes ?? string.Empty;
        data.Fields["Invoice_Status"] = invoice.Status;

        var withholding = invoice.WithholdingTaxEnabled ? invoice.WithholdingTaxAmount ?? 0m : 0m;
        var splitTax = invoice.SplitPayment ? invoice.TotalTax : 0m;
        data.Fields["Total_Net"] = ReportFormat.Amount(invoice.TotalNet, culture);
        data.Fields["Total_Discount"] = ReportFormat.OptionalAmount(invoice.DiscountAmount, culture);
        data.Fields["Total_Tax"] = ReportFormat.Amount(invoice.TotalTax, culture);
        data.Fields["Total_Gross"] = ReportFormat.Amount(invoice.TotalGross, culture);
        data.Fields["Total_Rounding"] = ReportFormat.OptionalAmount(invoice.RoundingAmount, culture);
        data.Fields["Total_Withholding"] = ReportFormat.OptionalAmount(withholding, culture);
        data.Fields["Total_StampDuty"] = invoice.StampDutyApplied ? ReportFormat.OptionalAmount(invoice.StampDutyAmount, culture) : string.Empty;
        data.Fields["Total_Payable"] = ReportFormat.Amount(invoice.TotalGross - withholding - splitTax, culture);
        data.Fields["Note_Legal"] = InvoiceLabels.LegalNotes(language, invoice);

        AddTotal(data, language, InvoiceTotalRow.Net, invoice.TotalNet, culture);
        AddTotal(data, language, InvoiceTotalRow.Discount, -invoice.DiscountAmount, culture);
        AddTotal(data, language, InvoiceTotalRow.Tax, invoice.TotalTax, culture);
        AddTotal(data, language, InvoiceTotalRow.Rounding, invoice.RoundingAmount, culture);
        AddTotal(data, language, InvoiceTotalRow.Gross, invoice.TotalGross, culture);
        AddTotal(data, language, InvoiceTotalRow.Withholding, -withholding, culture);
        AddTotal(data, language, InvoiceTotalRow.SplitPayment, -splitTax, culture);
        AddTotal(data, language, InvoiceTotalRow.StampDuty, invoice.StampDutyApplied ? invoice.StampDutyAmount ?? 0m : 0m, culture);

        if (withholding != 0m || splitTax != 0m)
        {
            AddTotal(data, language, InvoiceTotalRow.Payable, invoice.TotalGross - withholding - splitTax, culture);
        }
    }

    private static void AddTotal(ReportData data, string language, InvoiceTotalRow row, decimal amount, CultureInfo culture)
    {
        var mandatory = row is InvoiceTotalRow.Net or InvoiceTotalRow.Tax or InvoiceTotalRow.Gross or InvoiceTotalRow.Payable;

        if (amount == 0m && !mandatory)
        {
            return;
        }

        data.AddRow("Totals", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Total_Label"] = InvoiceLabels.Total(language, row),
            ["Total_Value"] = ReportFormat.Amount(amount, culture),
        });
    }

    private static void AddItems(ReportData data, IEnumerable<InvoiceItem> items, CultureInfo culture)
    {
        foreach (var item in items)
        {
            data.AddRow("Items", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Item_Description"] = item.Description,
                ["Item_Quantity"] = ReportFormat.Quantity(item.Quantity, culture),
                ["Item_Uom"] = item.Uom,
                ["Item_UnitPrice"] = ReportFormat.Amount(item.UnitPrice, culture),
                ["Item_Discount"] = item.DiscountPercentage == 0m ? string.Empty : ReportFormat.Percent(item.DiscountPercentage, culture),
                ["Item_TaxRate"] = string.IsNullOrWhiteSpace(item.VatNature) ? ReportFormat.Percent(item.TaxRate, culture) : item.VatNature,
                ["Item_Total"] = ReportFormat.Amount(item.LineTotal, culture),
            });
        }

        var summary = items
            .GroupBy(static i => (i.TaxRate, Nature: i.VatNature ?? string.Empty))
            .OrderByDescending(static g => g.Key.TaxRate)
            .ThenBy(static g => g.Key.Nature, StringComparer.Ordinal);

        foreach (var group in summary)
        {
            var taxable = group.Sum(static i => i.LineTotal);
            var tax = Math.Round(taxable * group.Key.TaxRate / 100m, 2, MidpointRounding.AwayFromZero);
            data.AddRow("VatSummary", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Vat_Rate"] = ReportFormat.Percent(group.Key.TaxRate, culture),
                ["Vat_Nature"] = group.Key.Nature,
                ["Vat_Taxable"] = ReportFormat.Amount(taxable, culture),
                ["Vat_Tax"] = ReportFormat.Amount(tax, culture),
            });
        }
    }

    private async Task<ReportData?> BuildInternalAsync(Guid entityId, CultureInfo culture, CancellationToken cancellationToken)
    {
        var invoice = await unitOfWork.Repository<Invoice>().GetByIdAsync(entityId, cancellationToken).ConfigureAwait(false);

        if (invoice is null)
        {
            return null;
        }

        var tenant = await unitOfWork.Repository<Tenant>().GetByIdAsync(invoice.TenantId, cancellationToken).ConfigureAwait(false);
        var contact = await unitOfWork.Repository<Contact>().GetByIdAsync(invoice.ContactId, cancellationToken).ConfigureAwait(false);
        var items = await unitOfWork.Repository<InvoiceItem>().FindAsync(i => i.InvoiceId == invoice.Id, cancellationToken).ConfigureAwait(false);
        var qrBills = await unitOfWork.Repository<InvoiceQrBillData>().FindAsync(q => q.InvoiceId == invoice.Id, cancellationToken).ConfigureAwait(false);

        ReportData data = new()
        {
            SuggestedFileName = ReportFormat.SafeFileName($"{InvoiceLabels.Title(ReportCulture.Language(culture), creditNote: false)}_{invoice.InvoiceNumber}.pdf"),
        };

        AddCompany(data, tenant);
        AddCustomer(data, contact, tenant?.Country ?? "IT");
        AddHeaderAndTotals(data, invoice, culture);
        AddItems(data, [.. items.Where(static i => i.DeletedAt is null).OrderBy(static i => i.CreatedAt)], culture);

        var qrBill = qrBills.FirstOrDefault(static q => string.IsNullOrWhiteSpace(q.ValidationErrors) && !string.IsNullOrWhiteSpace(q.QrBillSvg));
        data.SwissQrBillSvg = qrBill?.QrBillSvg;
        data.Fields["QrBill_Reference"] = qrBill?.PaymentReference ?? string.Empty;

        return data;
    }
}
