// <copyright file="PayrollTemplates.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Syncfusion.DocIO.DLS;

namespace FatturaFacile.Infrastructure.Reporting;

/// <summary>
/// Standard payslip and salary certificate templates.
/// </summary>
public static class PayrollTemplates
{
    private static readonly string[] _addressFields = ["Employee_Name", "Employee_AddressLine", "Employee_CityLine"];

    private static readonly string[] _certificateEmployeeFields =
        ["Employee_Name", "Employee_AddressLine", "Employee_CityLine", "Employee_AhvNumber", "Employee_DateOfBirth"];

    /// <summary>
    /// Gets the field reference of payslips.
    /// </summary>
    internal static string PayslipFields =>
        string.Join(
            Environment.NewLine,
            "Campi semplici:",
            "  Company_Name, Company_AddressLine, Company_CityLine, Company_UidNumber",
            "  Employee_Name, Employee_Number, Employee_AddressLine, Employee_CityLine, Employee_AhvNumber, Employee_DateOfBirth,",
            "  Employee_HireDate, Employee_TerminationDate, Employee_JobTitle, Employee_Iban, Employee_Permit",
            "  Period, PaymentDate, PaidDays, Gross, Deductions, Net, Payout, EmployerContributions,",
            "  AhvBasis, AlvBasis, UvgBasis, BvgInsuredSalary, WithholdingTax",
            string.Empty,
            "Gruppo Lines (salario, prestazioni, spese e trattenute) e gruppo EmployerLines (contributi del datore di lavoro):",
            "  Line_Code, Line_Description, Line_Quantity, Line_Rate, Line_Amount");

    /// <summary>
    /// Gets the field reference of salary certificates.
    /// </summary>
    internal static string SalaryCertificateFields =>
        string.Join(
            Environment.NewLine,
            "Campi semplici (oltre ai campi azienda e dipendente delle buste paga):",
            "  Year, PeriodFrom, PeriodTo, IssueDate, Remarks",
            "  Box1 (salario), Box2 (prestazioni salariali accessorie), Box3 (prestazioni non periodiche), Box4 (prestazioni in capitale),",
            "  Box7 (altre prestazioni), Box8 (salario lordo), Box9 (contributi AVS/AI/IPG/AD/AINP), Box10 (LPP ordinari),",
            "  Box11 (salario netto), Box12 (imposta alla fonte), Box13 (spese)",
            string.Empty,
            "Nota: il modulo ufficiale 11 va stampato con l'impaginazione dell'AFC; questo modello riporta gli stessi riquadri.");

    private static IReadOnlyList<(string Label, string Field)> CertificateBoxesItalian =>
    [
        ("1. Salario", "Box1"),
        ("2. Prestazioni salariali accessorie", "Box2"),
        ("3. Prestazioni non periodiche", "Box3"),
        ("4. Prestazioni in capitale", "Box4"),
        ("7. Altre prestazioni", "Box7"),
        ("8. Salario lordo totale", "Box8"),
        ("9. Contributi AVS/AI/IPG/AD/AINP", "Box9"),
        ("10.1 Previdenza professionale, contributi ordinari", "Box10"),
        ("11. Salario netto", "Box11"),
        ("12. Imposta alla fonte trattenuta", "Box12"),
        ("13. Indennità per spese", "Box13"),
    ];

    private static IReadOnlyList<(string Label, string Field)> CertificateBoxesGerman =>
    [
        ("1. Lohn", "Box1"),
        ("2. Gehaltsnebenleistungen", "Box2"),
        ("3. Unregelmässige Leistungen", "Box3"),
        ("4. Kapitalleistungen", "Box4"),
        ("7. Andere Leistungen", "Box7"),
        ("8. Bruttolohn total", "Box8"),
        ("9. Beiträge AHV/IV/EO/ALV/NBUV", "Box9"),
        ("10.1 Berufliche Vorsorge, ordentliche Beiträge", "Box10"),
        ("11. Nettolohn", "Box11"),
        ("12. Quellensteuerabzug", "Box12"),
        ("13. Spesenvergütungen", "Box13"),
    ];

    /// <summary>
    /// Creates the standard payslip templates.
    /// </summary>
    /// <returns>The templates.</returns>
    public static IEnumerable<(string Name, byte[] Content)> Payslips() =>
    [
        ("Busta_paga_IT", Payslip(new PayslipTemplateLabels { Title = "Busta paga", Period = "Periodo", PaidDays = "Giorni pagati", Columns = ["Cod.", "Descrizione", "Base / Q.tà", "Aliquota", "Importo"], Net = "Salario netto", Payout = "Importo versato", Employer = "Contributi del datore di lavoro", Page = "Pagina" })),
        ("Lohnabrechnung_DE", Payslip(new PayslipTemplateLabels { Title = "Lohnabrechnung", Period = "Periode", PaidDays = "Bezahlte Tage", Columns = ["LA", "Bezeichnung", "Basis / Menge", "Ansatz", "Betrag"], Net = "Nettolohn", Payout = "Auszahlung", Employer = "Arbeitgeberbeiträge", Page = "Seite" })),
        ("Fiche_salaire_FR", Payslip(new PayslipTemplateLabels { Title = "Fiche de salaire", Period = "Période", PaidDays = "Jours payés", Columns = ["Code", "Désignation", "Base / Qté", "Taux", "Montant"], Net = "Salaire net", Payout = "Montant versé", Employer = "Cotisations de l'employeur", Page = "Page" })),
    ];

    /// <summary>
    /// Creates the standard salary certificate templates.
    /// </summary>
    /// <returns>The templates.</returns>
    public static IEnumerable<(string Name, byte[] Content)> SalaryCertificates() =>
    [
        ("Certificato_salario_IT", Certificate("Certificato di salario", "Periodo", CertificateBoxesItalian, "Pagina")),
        ("Lohnausweis_DE", Certificate("Lohnausweis", "Periode", CertificateBoxesGerman, "Seite")),
    ];

    private static byte[] Payslip(PayslipTemplateLabels labels)
    {
        var (document, section) = DefaultReportTemplates.NewDocument(labels.Page);

        using (document)
        {
            _ = DefaultReportTemplates.FieldLine(section, 13f, bold: true, "Company_Name");
            _ = DefaultReportTemplates.FieldLine(section, 9f, bold: false, "Company_AddressLine");
            var city = DefaultReportTemplates.FieldLine(section, 9f, bold: false, "Company_CityLine");
            city.ParagraphFormat.AfterSpacing = 18f;

            foreach (var field in _addressFields)
            {
                var line = DefaultReportTemplates.FieldLine(section, 10f, string.Equals(field, "Employee_Name", StringComparison.Ordinal), field);
                line.ParagraphFormat.LeftIndent = 280f;
            }

            var heading = section.AddParagraph();
            heading.ParagraphFormat.BeforeSpacing = 24f;
            DefaultReportTemplates.Text(heading, labels.Title + " ", 15f, bold: true);
            DefaultReportTemplates.Fields(heading, 15f, bold: true, "Period");

            var info = section.AddParagraph();
            DefaultReportTemplates.Text(info, "AVS: ", 8.5f, bold: false);
            DefaultReportTemplates.Fields(info, 8.5f, bold: false, "Employee_AhvNumber");
            DefaultReportTemplates.Text(info, "    " + labels.PaidDays + ": ", 8.5f, bold: false);
            DefaultReportTemplates.Fields(info, 8.5f, bold: false, "PaidDays");
            DefaultReportTemplates.Text(info, "    ", 8.5f, bold: false);
            DefaultReportTemplates.Fields(info, 8.5f, bold: false, "WithholdingTax");
            info.ParagraphFormat.AfterSpacing = 12f;

            DefaultReportTemplates.GroupTable(
                section,
                "Lines",
                labels.Columns,
                ["Line_Code", "Line_Description", "Line_Quantity", "Line_Rate", "Line_Amount"],
                [40f, 230f, 80f, 60f, 85f],
                rightAlignedFrom: 2);

            Total(section, labels.Net, "Net", bold: false);
            Total(section, labels.Payout, "Payout", bold: true);

            var transfer = section.AddParagraph();
            DefaultReportTemplates.Text(transfer, "IBAN ", 8.5f, bold: false);
            DefaultReportTemplates.Fields(transfer, 8.5f, bold: false, "Employee_Iban");

            var employerTitle = section.AddParagraph();
            employerTitle.ParagraphFormat.BeforeSpacing = 18f;
            DefaultReportTemplates.Text(employerTitle, labels.Employer, 8.5f, bold: true);
            DefaultReportTemplates.GroupTable(
                section,
                "EmployerLines",
                labels.Columns,
                ["Line_Code", "Line_Description", "Line_Quantity", "Line_Rate", "Line_Amount"],
                [40f, 230f, 80f, 60f, 85f],
                rightAlignedFrom: 2);

            return DefaultReportTemplates.Save(document);
        }
    }

    private static void Total(IWSection section, string label, string field, bool bold)
    {
        var paragraph = section.AddParagraph();
        paragraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
        paragraph.ParagraphFormat.BeforeSpacing = 6f;
        DefaultReportTemplates.Text(paragraph, label + "   ", 10f, bold);
        DefaultReportTemplates.Fields(paragraph, 10f, bold, field);
    }

    private static byte[] Certificate(string title, string period, IReadOnlyList<(string Label, string Field)> boxes, string page)
    {
        var (document, section) = DefaultReportTemplates.NewDocument(page);

        using (document)
        {
            var heading = section.AddParagraph();
            DefaultReportTemplates.Text(heading, title + " ", 15f, bold: true);
            DefaultReportTemplates.Fields(heading, 15f, bold: true, "Year");
            heading.ParagraphFormat.AfterSpacing = 12f;

            foreach (var field in _certificateEmployeeFields)
            {
                _ = DefaultReportTemplates.FieldLine(section, 10f, string.Equals(field, "Employee_Name", StringComparison.Ordinal), field);
            }

            var periodLine = section.AddParagraph();
            periodLine.ParagraphFormat.BeforeSpacing = 8f;
            DefaultReportTemplates.Text(periodLine, period + ": ", 9f, bold: false);
            DefaultReportTemplates.Fields(periodLine, 9f, bold: false, "PeriodFrom");
            DefaultReportTemplates.Text(periodLine, " - ", 9f, bold: false);
            DefaultReportTemplates.Fields(periodLine, 9f, bold: false, "PeriodTo");
            periodLine.ParagraphFormat.AfterSpacing = 12f;

            var table = section.AddTable();
            table.ResetCells(boxes.Count, 2);
            table.TableFormat.Borders.BorderType = BorderStyle.Single;

            for (var row = 0; row < boxes.Count; row++)
            {
                table[row, 0].Width = 380f;
                table[row, 1].Width = 115f;
                DefaultReportTemplates.Text(table[row, 0].AddParagraph(), boxes[row].Label, 9f, bold: false);
                var value = table[row, 1].AddParagraph();
                value.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
                DefaultReportTemplates.Fields(value, 9f, bold: false, boxes[row].Field);
            }

            var remarks = section.AddParagraph();
            remarks.ParagraphFormat.BeforeSpacing = 12f;
            DefaultReportTemplates.Text(remarks, "15. ", 9f, bold: true);
            DefaultReportTemplates.Fields(remarks, 9f, bold: false, "Remarks");

            var signature = section.AddParagraph();
            signature.ParagraphFormat.BeforeSpacing = 24f;
            DefaultReportTemplates.Fields(signature, 9f, bold: false, "Company_Name", "Company_AddressLine", "Company_CityLine");
            var date = section.AddParagraph();
            DefaultReportTemplates.Fields(date, 9f, bold: false, "IssueDate");

            return DefaultReportTemplates.Save(document);
        }
    }
}
