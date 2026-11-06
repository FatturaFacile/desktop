// <copyright file="DefaultReportTemplates.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Text;
using FatturaFacile.Application.Common.Reporting;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;

namespace FatturaFacile.Infrastructure.Reporting;

/// <summary>
/// Generates the standard Word templates (merge fields «Field» and repeating regions «TableStart:Group» … «TableEnd:Group»).
/// </summary>
public static class DefaultReportTemplates
{
    private const string FontName = "Arial";

    private static readonly string[] _customerFields = ["Customer_Name", "Customer_AddressLine", "Customer_CityLine", "Customer_VatNumber"];

    /// <summary>
    /// Gets the field reference of invoices.
    /// </summary>
    internal static string InvoiceFields =>
        string.Join(
            Environment.NewLine,
            "Campi semplici:",
            "  Company_Name, Company_AddressLine, Company_CityLine, Company_Country, Company_VatNumber, Company_FiscalCode,",
            "  Company_UidNumber, Company_Iban, Company_Pec",
            "  Customer_Name, Customer_AddressLine, Customer_CityLine, Customer_Country, Customer_VatNumber, Customer_FiscalCode,",
            "  Customer_SdiCode, Customer_Pec, Customer_Email",
            "  Document_Title, Invoice_Number, Invoice_Date, Invoice_DueDate, Invoice_Currency, Invoice_PaymentMethod,",
            "  Invoice_BankIban, Invoice_Notes, Invoice_Status, Note_Legal, QrBill_Reference",
            "  Total_Net, Total_Discount, Total_Tax, Total_Gross, Total_Rounding, Total_Withholding, Total_StampDuty, Total_Payable",
            string.Empty,
            "Gruppo Items (righe della fattura):",
            "  Item_Description, Item_Quantity, Item_Uom, Item_UnitPrice, Item_Discount, Item_TaxRate, Item_Total",
            string.Empty,
            "Gruppo VatSummary (riepilogo IVA):",
            "  Vat_Rate, Vat_Nature, Vat_Taxable, Vat_Tax",
            string.Empty,
            "Gruppo Totals (solo i totali presenti, con etichetta nella lingua del modello):",
            "  Total_Label, Total_Value",
            string.Empty,
            "Per le fatture svizzere la sezione pagamento QR viene aggiunta automaticamente su una pagina separata.");

    /// <summary>
    /// Creates the standard templates of a document type.
    /// </summary>
    /// <param name="documentType">The document type.</param>
    /// <returns>The template names and .docx contents.</returns>
    public static IEnumerable<(string Name, byte[] Content)> Create(ReportDocumentType documentType)
    {
        return documentType switch
        {
            ReportDocumentType.Invoice =>
            [
                ("Fattura_IT", Invoice(TemplateLabels.Italian)),
                ("Fattura_EN", Invoice(TemplateLabels.English)),
                ("Rechnung_DE", Invoice(TemplateLabels.German)),
                ("Facture_FR", Invoice(TemplateLabels.French)),
            ],
            ReportDocumentType.Payslip => PayrollTemplates.Payslips(),
            ReportDocumentType.SalaryCertificate => PayrollTemplates.SalaryCertificates(),
            ReportDocumentType.TimeSheet => TimeSheetTemplates.Create(),
            _ => [],
        };
    }

    /// <summary>
    /// Returns the list of merge fields and groups available for a document type, written next to the templates.
    /// </summary>
    /// <param name="documentType">The document type.</param>
    /// <returns>The reference text.</returns>
    public static string FieldReference(ReportDocumentType documentType)
    {
        StringBuilder text = new();
        _ = text.AppendLine("MODELLI DI STAMPA - CAMPI DISPONIBILI");
        _ = text.AppendLine("=====================================");
        _ = text.AppendLine();
        _ = text.AppendLine("Per creare un nuovo modello: duplicare un file .docx di questa cartella (anche dal programma),");
        _ = text.AppendLine("rinominarlo (il suffisso _IT, _DE, _FR, _EN decide lingua e formato di date e importi) e modificarlo con Word:");
        _ = text.AppendLine("logo, colori, testi, posizione dei campi. I campi si inseriscono con Inserisci > Parti rapide > Campo > MergeField.");
        _ = text.AppendLine("Le righe ripetute stanno in una riga di tabella che inizia con «TableStart:Gruppo» e finisce con «TableEnd:Gruppo».");
        _ = text.AppendLine();

        var fields = documentType switch
        {
            ReportDocumentType.Invoice => InvoiceFields,
            ReportDocumentType.Payslip => PayrollTemplates.PayslipFields,
            ReportDocumentType.SalaryCertificate => PayrollTemplates.SalaryCertificateFields,
            ReportDocumentType.TimeSheet => TimeSheetTemplates.Fields,
            _ => string.Empty,
        };

        _ = text.Append(fields);
        return text.ToString();
    }

    /// <summary>
    /// Saves a document as .docx.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <returns>The .docx bytes.</returns>
    internal static byte[] Save(WordDocument document)
    {
        using MemoryStream stream = new();
        document.Save(stream, FormatType.Docx);
        return stream.ToArray();
    }

    /// <summary>
    /// Creates an A4 document with Arial as default font and a page-number footer.
    /// </summary>
    /// <param name="pageLabel">The "Page" label.</param>
    /// <returns>The document and its section.</returns>
    internal static (WordDocument Document, IWSection Section) NewDocument(string pageLabel)
    {
        WordDocument document = new();
        var section = document.AddSection();
        section.PageSetup.PageSize = PageSize.A4;
        section.PageSetup.Margins.All = 50f;

        var footer = section.HeadersFooters.Footer.AddParagraph();
        footer.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
        Text(footer, pageLabel + " ", 8f, bold: false);
        _ = footer.AppendField("Page", FieldType.FieldPage);
        return (document, section);
    }

    /// <summary>
    /// Appends formatted text to a paragraph.
    /// </summary>
    /// <param name="paragraph">The paragraph.</param>
    /// <param name="value">The text.</param>
    /// <param name="size">The font size.</param>
    /// <param name="bold">Whether the text is bold.</param>
    internal static void Text(IWParagraph paragraph, string value, float size, bool bold)
    {
        var range = paragraph.AppendText(value);
        range.CharacterFormat.FontName = FontName;
        range.CharacterFormat.FontSize = size;
        range.CharacterFormat.Bold = bold;
    }

    /// <summary>
    /// Appends merge fields (one per name, separated by a space) to a paragraph.
    /// </summary>
    /// <param name="paragraph">The paragraph.</param>
    /// <param name="size">The font size.</param>
    /// <param name="bold">Whether the fields are bold.</param>
    /// <param name="names">The merge field names.</param>
    internal static void Fields(IWParagraph paragraph, float size, bool bold, params string[] names)
    {
        for (var i = 0; i < names.Length; i++)
        {
            if (i > 0)
            {
                Text(paragraph, " ", size, bold);
            }

            var field = (WMergeField)paragraph.AppendField(names[i], FieldType.FieldMergeField);
            field.CharacterFormat.FontName = FontName;
            field.CharacterFormat.FontSize = size;
            field.CharacterFormat.Bold = bold;
        }
    }

    /// <summary>
    /// Adds a paragraph with merge fields.
    /// </summary>
    /// <param name="section">The section.</param>
    /// <param name="size">The font size.</param>
    /// <param name="bold">Whether the fields are bold.</param>
    /// <param name="names">The merge field names.</param>
    /// <returns>The paragraph.</returns>
    internal static IWParagraph FieldLine(IWSection section, float size, bool bold, params string[] names)
    {
        var paragraph = section.AddParagraph();
        paragraph.ParagraphFormat.AfterSpacing = 0f;
        Fields(paragraph, size, bold, names);
        return paragraph;
    }

    /// <summary>
    /// Adds a table whose first row contains headings and whose second row repeats for every row of a group.
    /// </summary>
    /// <param name="section">The section.</param>
    /// <param name="group">The group name.</param>
    /// <param name="headings">The column headings.</param>
    /// <param name="fields">The merge field of each column.</param>
    /// <param name="widths">The column widths in points.</param>
    /// <param name="rightAlignedFrom">The first right-aligned (numeric) column.</param>
    internal static void GroupTable(IWSection section, string group, IReadOnlyList<string> headings, IReadOnlyList<string> fields, IReadOnlyList<float> widths, int rightAlignedFrom)
    {
        var table = section.AddTable();
        table.ResetCells(2, fields.Count);
        table.TableFormat.Borders.BorderType = BorderStyle.None;
        table.TableFormat.Borders.Horizontal.BorderType = BorderStyle.Single;
        table.TableFormat.Borders.Bottom.BorderType = BorderStyle.Single;

        for (var column = 0; column < fields.Count; column++)
        {
            var alignment = column >= rightAlignedFrom ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            table[0, column].Width = widths[column];
            table[1, column].Width = widths[column];

            var heading = table[0, column].AddParagraph();
            heading.ParagraphFormat.HorizontalAlignment = alignment;
            Text(heading, headings[column], 8.5f, bold: true);

            var cell = table[1, column].AddParagraph();
            cell.ParagraphFormat.HorizontalAlignment = alignment;

            if (column == 0)
            {
                Fields(cell, 9f, bold: false, "TableStart:" + group);
            }

            Fields(cell, 9f, bold: false, fields[column]);

            if (column == fields.Count - 1)
            {
                Fields(cell, 9f, bold: false, "TableEnd:" + group);
            }
        }

        table.Rows[0].IsHeader = true;
    }

    private static byte[] Invoice(TemplateLabels labels)
    {
        var (document, section) = NewDocument(labels.Page);

        using (document)
        {
            // Company (sender) block: replace or add the logo here in Word.
            _ = FieldLine(section, 14f, bold: true, "Company_Name");
            _ = FieldLine(section, 9f, bold: false, "Company_AddressLine");
            _ = FieldLine(section, 9f, bold: false, "Company_CityLine");
            var vat = section.AddParagraph();
            Text(vat, labels.VatNumber + ": ", 9f, bold: false);
            Fields(vat, 9f, bold: false, "Company_VatNumber");
            vat.ParagraphFormat.AfterSpacing = 18f;

            // Customer block, right side (window envelope position).
            var customerHeading = section.AddParagraph();
            customerHeading.ParagraphFormat.LeftIndent = 280f;
            Text(customerHeading, labels.Customer, 8f, bold: false);

            foreach (var fieldName in _customerFields)
            {
                var line = FieldLine(section, 10f, string.Equals(fieldName, "Customer_Name", StringComparison.Ordinal), fieldName);
                line.ParagraphFormat.LeftIndent = 280f;
            }

            var title = section.AddParagraph();
            title.ParagraphFormat.BeforeSpacing = 24f;
            Fields(title, 16f, bold: true, "Document_Title");
            Text(title, " ", 16f, bold: true);
            Fields(title, 16f, bold: true, "Invoice_Number");

            var dates = section.AddParagraph();
            Text(dates, labels.Date + ": ", 9f, bold: false);
            Fields(dates, 9f, bold: false, "Invoice_Date");
            Text(dates, "    " + labels.DueDate + ": ", 9f, bold: false);
            Fields(dates, 9f, bold: false, "Invoice_DueDate");
            Text(dates, "    ", 9f, bold: false);
            Fields(dates, 9f, bold: false, "Invoice_Currency");
            dates.ParagraphFormat.AfterSpacing = 12f;

            GroupTable(
                section,
                "Items",
                labels.Columns,
                ["Item_Description", "Item_Quantity", "Item_Uom", "Item_UnitPrice", "Item_Discount", "Item_TaxRate", "Item_Total"],
                [190f, 40f, 35f, 65f, 45f, 45f, 75f],
                rightAlignedFrom: 1);

            var summaryTitle = section.AddParagraph();
            summaryTitle.ParagraphFormat.BeforeSpacing = 12f;
            Text(summaryTitle, labels.VatSummary[0], 9f, bold: true);
            GroupTable(
                section,
                "VatSummary",
                [labels.VatSummary[1], labels.VatSummary[2], labels.VatSummary[3], labels.VatSummary[4]],
                ["Vat_Rate", "Vat_Nature", "Vat_Taxable", "Vat_Tax"],
                [60f, 60f, 90f, 90f],
                rightAlignedFrom: 2);

            var spacer = section.AddParagraph();
            spacer.ParagraphFormat.AfterSpacing = 6f;
            GroupTable(section, "Totals", [string.Empty, string.Empty], ["Total_Label", "Total_Value"], [400f, 95f], rightAlignedFrom: 1);

            var payment = section.AddParagraph();
            payment.ParagraphFormat.BeforeSpacing = 12f;
            Text(payment, labels.Payment + ": ", 9f, bold: true);
            Fields(payment, 9f, bold: false, "Invoice_PaymentMethod", "Invoice_BankIban");

            var notes = section.AddParagraph();
            Text(notes, labels.Notes + ": ", 9f, bold: true);
            Fields(notes, 9f, bold: false, "Invoice_Notes");
            _ = FieldLine(section, 8f, bold: false, "Note_Legal");

            return Save(document);
        }
    }
}
