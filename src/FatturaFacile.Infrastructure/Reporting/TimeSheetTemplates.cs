// <copyright file="TimeSheetTemplates.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Reporting;

/// <summary>
/// Standard monthly time sheet templates (registers kept for the labour inspectorate, art. 73 OLL 1).
/// </summary>
public static class TimeSheetTemplates
{
    /// <summary>
    /// Gets the field reference of time sheets.
    /// </summary>
    internal static string Fields =>
        string.Join(
            Environment.NewLine,
            "Campi semplici (oltre ai campi azienda e dipendente delle buste paga):",
            "  PeriodFrom, PeriodTo, RecordingMode, Worked, Target, Credited, Balance,",
            "  StatutoryOvertime, StatutoryOvertimeYear, StatutoryOvertimeCap, Issues, PrintDate",
            string.Empty,
            "Gruppo Days: Day_Date, Day_In, Day_Out, Day_Break, Day_Worked, Day_Target, Day_Balance, Day_Absence, Day_Issues",
            "Gruppo Weeks: Week_Start, Week_Worked, Week_Maximum, Week_Overtime",
            string.Empty,
            "I documenti vanno conservati almeno cinque anni (art. 73 cpv. 2 OLL 1).");

    /// <summary>
    /// Creates the standard time sheet templates.
    /// </summary>
    /// <returns>The templates.</returns>
    public static IEnumerable<(string Name, byte[] Content)> Create() =>
    [
        ("Foglio_presenze_IT", TimeSheet("Foglio presenze", "Periodo", ["Giorno", "Entrata", "Uscita", "Pause", "Lavorato", "Previsto", "Saldo", "Assenza", "Osservazioni"], ["Settimana dal", "Lavorato", "Massimo legale", "Straordinario"], ["Lavorato", "Previsto", "Assenze pagate", "Saldo", "Straordinario nel periodo", "Straordinario nell'anno / limite"], "Firma del collaboratore", "Pagina")),
        ("Arbeitszeitrapport_DE", TimeSheet("Arbeitszeitrapport", "Periode", ["Tag", "Beginn", "Ende", "Pausen", "Gearbeitet", "Soll", "Saldo", "Absenz", "Bemerkungen"], ["Woche ab", "Gearbeitet", "Höchstarbeitszeit", "Überzeit"], ["Gearbeitet", "Soll", "Bezahlte Absenzen", "Saldo", "Überzeit in der Periode", "Überzeit im Jahr / Grenze"], "Unterschrift Mitarbeiter", "Seite")),
        ("Feuille_de_temps_FR", TimeSheet("Relevé du temps de travail", "Période", ["Jour", "Début", "Fin", "Pauses", "Travaillé", "Prévu", "Solde", "Absence", "Remarques"], ["Semaine du", "Travaillé", "Maximum légal", "Travail supplémentaire"], ["Travaillé", "Prévu", "Absences payées", "Solde", "Travail supplémentaire période", "Travail supplémentaire année / limite"], "Signature du collaborateur", "Page")),
    ];

    private static byte[] TimeSheet(string title, string period, string[] dayColumns, string[] weekColumns, string[] totals, string signature, string page)
    {
        var (document, section) = DefaultReportTemplates.NewDocument(page);

        using (document)
        {
            section.PageSetup.Orientation = Syncfusion.DocIO.DLS.PageOrientation.Landscape;
            section.PageSetup.Margins.All = 36f;

            var heading = section.AddParagraph();
            DefaultReportTemplates.Text(heading, title + "  ", 14f, bold: true);
            DefaultReportTemplates.Fields(heading, 14f, bold: true, "Employee_Name");

            var info = section.AddParagraph();
            DefaultReportTemplates.Fields(info, 9f, bold: false, "Company_Name");
            DefaultReportTemplates.Text(info, "  -  " + period + ": ", 9f, bold: false);
            DefaultReportTemplates.Fields(info, 9f, bold: false, "PeriodFrom");
            DefaultReportTemplates.Text(info, " - ", 9f, bold: false);
            DefaultReportTemplates.Fields(info, 9f, bold: false, "PeriodTo");
            DefaultReportTemplates.Text(info, "  (", 9f, bold: false);
            DefaultReportTemplates.Fields(info, 9f, bold: false, "RecordingMode");
            DefaultReportTemplates.Text(info, ")", 9f, bold: false);
            info.ParagraphFormat.AfterSpacing = 8f;

            DefaultReportTemplates.GroupTable(
                section,
                "Days",
                dayColumns,
                ["Day_Date", "Day_In", "Day_Out", "Day_Break", "Day_Worked", "Day_Target", "Day_Balance", "Day_Absence", "Day_Issues"],
                [55f, 40f, 40f, 40f, 50f, 50f, 45f, 70f, 380f],
                rightAlignedFrom: 1);

            var weekTitle = section.AddParagraph();
            weekTitle.ParagraphFormat.BeforeSpacing = 10f;
            DefaultReportTemplates.GroupTable(section, "Weeks", weekColumns, ["Week_Start", "Week_Worked", "Week_Maximum", "Week_Overtime"], [110f, 80f, 100f, 100f], rightAlignedFrom: 1);

            string[] totalFields = ["Worked", "Target", "Credited", "Balance", "StatutoryOvertime", "StatutoryOvertimeYear"];

            for (var i = 0; i < totals.Length; i++)
            {
                var line = section.AddParagraph();
                line.ParagraphFormat.BeforeSpacing = i == 0 ? 10f : 0f;
                DefaultReportTemplates.Text(line, totals[i] + ": ", 9f, bold: true);
                DefaultReportTemplates.Fields(line, 9f, bold: false, totalFields[i]);

                if (i == totals.Length - 1)
                {
                    DefaultReportTemplates.Text(line, " / ", 9f, bold: false);
                    DefaultReportTemplates.Fields(line, 9f, bold: false, "StatutoryOvertimeCap");
                }
            }

            var issues = DefaultReportTemplates.FieldLine(section, 8.5f, bold: false, "Issues");
            issues.ParagraphFormat.BeforeSpacing = 6f;

            var sign = section.AddParagraph();
            sign.ParagraphFormat.BeforeSpacing = 24f;
            DefaultReportTemplates.Text(sign, signature + ": ______________________________     ", 9f, bold: false);
            DefaultReportTemplates.Fields(sign, 9f, bold: false, "PrintDate");

            return DefaultReportTemplates.Save(document);
        }
    }
}
