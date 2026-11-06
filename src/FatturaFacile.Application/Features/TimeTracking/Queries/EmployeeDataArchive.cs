// <copyright file="EmployeeDataArchive.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using FatturaFacile.Application.Common.Export;
using FatturaFacile.Application.TimeTracking;

namespace FatturaFacile.Application.Features.TimeTracking.Queries;

/// <summary>
/// Writes the ZIP archive of an employee export (JSON for portability, CSV for reading).
/// </summary>
internal static class EmployeeDataArchive
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    /// <summary>
    /// Builds the archive.
    /// </summary>
    /// <param name="content">The collected data.</param>
    /// <returns>The ZIP archive.</returns>
    public static byte[] Build(EmployeeDataContent content)
    {
        using var buffer = new MemoryStream();

        using (var zip = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddText(zip, "LEGGIMI.txt", Readme(content));
            AddJson(zip, "dati-personali.json", new { content.Employee, FamilyMembers = content.Children, content.Absences });
            AddJson(zip, "durata-del-lavoro.json", new { content.ClockEvents, content.DailyRecords, ChangeHistory = content.Audit });
            AddText(zip, "timbrature.csv", ClockCsv(content));
            AddText(zip, "registrazioni-giornaliere.csv", DailyCsv(content));
            AddText(zip, "storico-modifiche.csv", AuditCsv(content));

            if (content.Report is { } report)
            {
                AddText(zip, "riepilogo-giornaliero.csv", ReportCsv(report));
            }

            if (content.IncludesPayroll)
            {
                AddJson(
                    zip,
                    "paghe.json",
                    new
                    {
                        Payslips = content.Payslips.OrderBy(static p => p.Year).ThenBy(static p => p.Month).Select(p => new
                        {
                            Payslip = p,
                            Lines = content.PayslipLines.Where(l => l.PayslipId == p.Id).OrderBy(static l => l.SortOrder),
                        }),
                        content.PayrollInputs,
                        content.SalaryCertificates,
                    });
                AddText(zip, "buste-paga.csv", PayslipCsv(content));
            }
        }

        return buffer.ToArray();
    }

    private static void AddJson(ZipArchive zip, string name, object value) => AddText(zip, name, JsonSerializer.Serialize(value, JsonOptions));

    private static void AddText(ZipArchive zip, string name, string text)
    {
        var entry = zip.CreateEntry(name, CompressionLevel.Optimal);
        using var stream = entry.Open();
        stream.Write(Utf8WithBom.GetPreamble());
        stream.Write(Utf8WithBom.GetBytes(text));
    }

    private static string Readme(EmployeeDataContent content)
    {
        var employee = content.Employee;
        List<string> lines =
        [
            $"Dati personali di {employee.FirstName} {employee.LastName} ({employee.EmployeeNumber})",
            string.Create(CultureInfo.InvariantCulture, $"Esportati il {DateTime.Now:dd.MM.yyyy HH:mm} da FatturaFacile per {content.CompanyName}."),
            string.Empty,
            "Diritto d'accesso e portabilità dei dati: art. 25 e 28 LPD (Svizzera), art. 15 e 20 GDPR (UE).",
            string.Empty,
            "Contenuto:",
            "- dati-personali.json: anagrafica, familiari e assenze",
            "- durata-del-lavoro.json, timbrature.csv, registrazioni-giornaliere.csv: registrazione della durata del lavoro (art. 46 LL, art. 73 OLL 1)",
            "- storico-modifiche.csv: chi ha modificato le registrazioni, quando e perché",
            "- riepilogo-giornaliero.csv: ore lavorate, pause, ore teoriche e saldo per giorno",
        ];

        lines.Add(content.IncludesPayroll
            ? "- paghe.json, buste-paga.csv: buste paga, voci variabili e certificati di salario"
            : "I dati salariali non sono inclusi: si possono richiedere all'ufficio del personale.");
        lines.Add(string.Empty);
        lines.Add("Le registrazioni della durata del lavoro sono conservate almeno 5 anni (art. 73 cpv. 2 OLL 1) e poi eliminate automaticamente.");
        lines.Add("I file CSV usano il punto e virgola come separatore, date nel formato AAAA-MM-GG e minuti come unità di tempo.");
        return string.Join(Environment.NewLine, lines);
    }

    private static string ClockCsv(EmployeeDataContent content)
    {
        CsvBuilder csv = new();
        csv.Row("Data e ora", "Tipo", "Origine", "Nota", "Eliminata il", "Motivo eliminazione");

        foreach (var clock in content.ClockEvents.OrderBy(static e => e.Timestamp))
        {
            csv.Row(clock.Timestamp, clock.EventType, clock.Source, clock.Note, clock.DeletedAt, clock.DeletionReason);
        }

        return csv.ToString();
    }

    private static string DailyCsv(EmployeeDataContent content)
    {
        CsvBuilder csv = new();
        csv.Row("Data", "Minuti lavorati", "Inizio", "Fine", "Nota");

        foreach (var record in content.DailyRecords.OrderBy(static r => r.Date))
        {
            csv.Row(record.Date, record.WorkedMinutes, record.StartTime, record.EndTime, record.Note);
        }

        return csv.ToString();
    }

    private static string AuditCsv(EmployeeDataContent content)
    {
        CsvBuilder csv = new();
        csv.Row("Data e ora", "Utente", "Azione", "Oggetto", "Prima", "Dopo", "Motivo");

        foreach (var entry in content.Audit.OrderBy(static a => a.Timestamp))
        {
            csv.Row(entry.Timestamp, entry.UserName, entry.Action, entry.EntityType, entry.Before, entry.After, entry.Reason);
        }

        return csv.ToString();
    }

    private static string ReportCsv(WorkingTimeReport report)
    {
        CsvBuilder csv = new();
        csv.Row("Data", "Entrata", "Uscita", "Minuti lavorati", "Pause", "Teorico", "Accreditato", "Saldo", "Notturno", "Domenica", "Assenza", "Segnalazioni");

        foreach (var day in report.Days)
        {
            csv.Row(day.Date, day.FirstIn, day.LastOut, day.WorkedMinutes, day.BreakMinutes, day.TargetMinutes, day.CreditedMinutes, day.BalanceMinutes, day.NightMinutes, day.SundayWork, day.Absence, day.IssuesText);
        }

        return csv.ToString();
    }

    private static string PayslipCsv(EmployeeDataContent content)
    {
        CsvBuilder csv = new();
        csv.Row("Anno", "Mese", "Lordo", "Trattenute", "Netto", "Pagamento", "Imposta alla fonte %");

        foreach (var payslip in content.Payslips.OrderBy(static p => p.Year).ThenBy(static p => p.Month))
        {
            csv.Row(payslip.Year, payslip.Month, payslip.GrossSalary, payslip.EmployeeDeductions, payslip.NetSalary, payslip.PayoutAmount, payslip.WithholdingTaxRatePercent);
        }

        return csv.ToString();
    }
}
