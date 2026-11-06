// <copyright file="TimeSheetReportDataBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Features.Reports.Builders;

/// <summary>
/// Builds the monthly time sheet of an employee (document for the labour inspectorate): the "Days" and "Weeks" groups,
/// the totals and the issues found.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TimeSheetReportDataBuilder"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="calculator">The working time calculator.</param>
public sealed class TimeSheetReportDataBuilder(IUnitOfWork unitOfWork, WorkingTimeCalculator calculator) : IPeriodReportDataBuilder
{
    /// <inheritdoc/>
    public ReportDocumentType DocumentType => ReportDocumentType.TimeSheet;

    /// <inheritdoc/>
    public Task<ReportData?> BuildAsync(Guid entityId, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        var first = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        return BuildAsync(entityId, first, first.AddMonths(1).AddDays(-1), culture, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<ReportData?> BuildAsync(Guid entityId, DateTime from, DateTime to, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(culture);

        return BuildInternalAsync(entityId, from, to, culture, cancellationToken);
    }

    private static string Hours(int minutes) => minutes == 0 ? string.Empty : WorkingTimeReport.FormatMinutes(minutes);

    private async Task<ReportData?> BuildInternalAsync(Guid employeeId, DateTime from, DateTime to, CultureInfo culture, CancellationToken cancellationToken)
    {
        var report = await calculator.EvaluateAsync(employeeId, from, to, cancellationToken).ConfigureAwait(false);

        if (report is null)
        {
            return null;
        }

        var employee = await unitOfWork.Repository<HrEmployee>().GetByIdAsync(employeeId, cancellationToken).ConfigureAwait(false);
        var tenant = employee is null ? null : await unitOfWork.Repository<Tenant>().GetByIdAsync(employee.TenantId, cancellationToken).ConfigureAwait(false);

        ReportData data = new()
        {
            SuggestedFileName = ReportFormat.SafeFileName(string.Create(CultureInfo.InvariantCulture, $"Presenze_{from:yyyy-MM}_{report.EmployeeName}.pdf")),
        };

        PayslipReportDataBuilder.AddCompanyAndEmployee(data, tenant, employee, culture);
        data.Fields["PeriodFrom"] = ReportFormat.Date(report.From, culture);
        data.Fields["PeriodTo"] = ReportFormat.Date(report.To, culture);
        data.Fields["RecordingMode"] = report.RecordingMode;
        data.Fields["Worked"] = WorkingTimeReport.FormatMinutes(report.WorkedMinutes);
        data.Fields["Target"] = WorkingTimeReport.FormatMinutes(report.TargetMinutes);
        data.Fields["Credited"] = WorkingTimeReport.FormatMinutes(report.CreditedMinutes);
        data.Fields["Balance"] = WorkingTimeReport.FormatMinutes(report.BalanceMinutes);
        data.Fields["StatutoryOvertime"] = WorkingTimeReport.FormatMinutes(report.StatutoryOvertimeMinutes);
        data.Fields["StatutoryOvertimeYear"] = WorkingTimeReport.FormatMinutes(report.StatutoryOvertimeYearMinutes);
        data.Fields["StatutoryOvertimeCap"] = WorkingTimeReport.FormatMinutes(report.StatutoryOvertimeCapMinutes);
        data.Fields["Issues"] = string.Join(Environment.NewLine, report.Issues);
        data.Fields["PrintDate"] = ReportFormat.Date(DateTime.Today, culture);

        foreach (var day in report.Days)
        {
            data.AddRow("Days", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Day_Date"] = day.Date.ToString("ddd dd.MM", culture),
                ["Day_In"] = day.FirstIn?.ToString("HH:mm", culture) ?? string.Empty,
                ["Day_Out"] = day.LastOut?.ToString("HH:mm", culture) ?? string.Empty,
                ["Day_Break"] = Hours(day.BreakMinutes),
                ["Day_Worked"] = Hours(day.WorkedMinutes),
                ["Day_Target"] = Hours(day.TargetMinutes),
                ["Day_Balance"] = Hours(day.BalanceMinutes),
                ["Day_Absence"] = day.Absence ?? string.Empty,
                ["Day_Issues"] = day.IssuesText,
            });
        }

        foreach (var week in report.Weeks)
        {
            data.AddRow("Weeks", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Week_Start"] = ReportFormat.Date(week.WeekStart, culture),
                ["Week_Worked"] = WorkingTimeReport.FormatMinutes(week.WorkedMinutes),
                ["Week_Maximum"] = WorkingTimeReport.FormatMinutes(week.LegalMaximumMinutes),
                ["Week_Overtime"] = Hours(week.StatutoryOvertimeMinutes),
            });
        }

        return data;
    }
}
