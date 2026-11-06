// <copyright file="LabourBookTemplates.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Reporting;

/// <summary>
/// Default template of the Libro Unico del Lavoro.
/// </summary>
public static class LabourBookTemplates
{
    /// <summary>
    /// Gets the list of the merge fields.
    /// </summary>
    internal static string Fields =>
        string.Join(
            Environment.NewLine,
            "Campi semplici: Company_Name, Company_AddressLine, Company_CityLine, Company_UidNumber, Period, InpsCompanyNumber, InailCode, PrintDate",
            string.Empty,
            "Gruppo Payslips: Pay_Employee, Pay_TaxCode, Pay_Contract, Pay_Days, Pay_Gross, Pay_Inps, Pay_Irpef, Pay_Net, Pay_EmployerInps, Pay_Tfr",
            "Gruppo Lines: Line_Employee, Line_Code, Line_Description, Line_Quantity, Line_Rate, Line_Amount",
            "Gruppo Attendance: Att_Employee, Att_Date, Att_Worked, Att_Overtime, Att_Absence",
            string.Empty,
            "Il LUL va stampato su fogli numerati (o tenuto in formato elettronico secondo le indicazioni dell'INL) e conservato 5 anni dall'ultima registrazione.");

    /// <summary>
    /// Creates the default templates.
    /// </summary>
    /// <returns>The templates.</returns>
    public static IEnumerable<(string Name, byte[] Content)> Create() => [("Libro_unico_IT", LabourBook())];

    private static byte[] LabourBook()
    {
        var (document, section) = DefaultReportTemplates.NewDocument("Pagina");

        using (document)
        {
            section.PageSetup.Orientation = Syncfusion.DocIO.DLS.PageOrientation.Landscape;
            section.PageSetup.Margins.All = 36f;

            var heading = section.AddParagraph();
            DefaultReportTemplates.Text(heading, "Libro Unico del Lavoro - ", 14f, bold: true);
            DefaultReportTemplates.Fields(heading, 14f, bold: true, "Period");

            var company = section.AddParagraph();
            DefaultReportTemplates.Fields(company, 9f, bold: false, "Company_Name");
            DefaultReportTemplates.Text(company, " - ", 9f, bold: false);
            DefaultReportTemplates.Fields(company, 9f, bold: false, "Company_AddressLine");
            DefaultReportTemplates.Text(company, " ", 9f, bold: false);
            DefaultReportTemplates.Fields(company, 9f, bold: false, "Company_CityLine");
            DefaultReportTemplates.Text(company, " - P.IVA ", 9f, bold: false);
            DefaultReportTemplates.Fields(company, 9f, bold: false, "Company_UidNumber");
            DefaultReportTemplates.Text(company, " - Matricola INPS ", 9f, bold: false);
            DefaultReportTemplates.Fields(company, 9f, bold: false, "InpsCompanyNumber");
            DefaultReportTemplates.Text(company, " - INAIL ", 9f, bold: false);
            DefaultReportTemplates.Fields(company, 9f, bold: false, "InailCode");
            company.ParagraphFormat.AfterSpacing = 8f;

            Title(section, "Sezione paga");
            DefaultReportTemplates.GroupTable(
                section,
                "Payslips",
                ["Lavoratore", "Codice fiscale", "CCNL / livello", "Giorni", "Lordo", "INPS dip.", "IRPEF", "Netto", "INPS azienda", "TFR"],
                ["Pay_Employee", "Pay_TaxCode", "Pay_Contract", "Pay_Days", "Pay_Gross", "Pay_Inps", "Pay_Irpef", "Pay_Net", "Pay_EmployerInps", "Pay_Tfr"],
                [120f, 100f, 70f, 40f, 60f, 55f, 55f, 60f, 60f, 50f],
                rightAlignedFrom: 3);

            Title(section, "Dettaglio delle voci");
            DefaultReportTemplates.GroupTable(
                section,
                "Lines",
                ["Lavoratore", "Codice", "Voce", "Quantità / base", "Tasso", "Importo"],
                ["Line_Employee", "Line_Code", "Line_Description", "Line_Quantity", "Line_Rate", "Line_Amount"],
                [140f, 50f, 260f, 80f, 60f, 80f],
                rightAlignedFrom: 3);

            Title(section, "Sezione presenze");
            DefaultReportTemplates.GroupTable(
                section,
                "Attendance",
                ["Lavoratore", "Giorno", "Ore lavorate", "Ore eccedenti", "Assenza"],
                ["Att_Employee", "Att_Date", "Att_Worked", "Att_Overtime", "Att_Absence"],
                [160f, 70f, 80f, 80f, 200f],
                rightAlignedFrom: 2);

            var footer = section.AddParagraph();
            footer.ParagraphFormat.BeforeSpacing = 12f;
            DefaultReportTemplates.Text(footer, "Stampato il ", 8f, bold: false);
            DefaultReportTemplates.Fields(footer, 8f, bold: false, "PrintDate");
            DefaultReportTemplates.Text(footer, " - Libro unico del lavoro, art. 39 D.L. 112/2008.", 8f, bold: false);
            return DefaultReportTemplates.Save(document);
        }
    }

    private static void Title(Syncfusion.DocIO.DLS.IWSection section, string text)
    {
        var paragraph = section.AddParagraph();
        paragraph.ParagraphFormat.BeforeSpacing = 10f;
        paragraph.ParagraphFormat.AfterSpacing = 4f;
        DefaultReportTemplates.Text(paragraph, text, 11f, bold: true);
    }
}
