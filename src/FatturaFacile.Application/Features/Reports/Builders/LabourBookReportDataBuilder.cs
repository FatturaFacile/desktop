// <copyright file="LabourBookReportDataBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Application.Payroll.Italian;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Features.Reports.Builders;

/// <summary>
/// Data of the Libro Unico del Lavoro of a payroll month (art. 39 D.L. 112/2008): pay section and attendance section.
/// The entity identifier is the payroll run.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="calculator">The working time evaluation (attendance section).</param>
public sealed class LabourBookReportDataBuilder(IUnitOfWork unitOfWork, WorkingTimeCalculator calculator) : IReportDataBuilder
{
    /// <inheritdoc />
    public ReportDocumentType DocumentType => ReportDocumentType.LabourBook;

    /// <inheritdoc />
    public Task<ReportData?> BuildAsync(Guid entityId, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(culture);

        return BuildInternalAsync(entityId, culture, cancellationToken);
    }

    private async Task<ReportData?> BuildInternalAsync(Guid runId, CultureInfo culture, CancellationToken cancellationToken)
    {
        var run = await unitOfWork.Repository<PayrollRun>().GetByIdAsync(runId, cancellationToken).ConfigureAwait(false);

        if (run is null)
        {
            return null;
        }

        var data = await ItalianPayrollPeriodData.LoadAsync(unitOfWork, run.Year, run.Month, cancellationToken).ConfigureAwait(false);
        var tenant = await unitOfWork.Repository<Tenant>().GetByIdAsync(run.TenantId, cancellationToken).ConfigureAwait(false);
        var first = new DateTime(run.Year, run.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var last = first.AddMonths(1).AddDays(-1);

        ReportData report = new()
        {
            SuggestedFileName = ReportFormat.SafeFileName(string.Create(CultureInfo.InvariantCulture, $"LUL_{run.Year}-{run.Month:00}.pdf")),
        };
        PayslipReportDataBuilder.AddCompanyAndEmployee(report, tenant, employee: null, culture);
        report.Fields["Period"] = culture.TextInfo.ToTitleCase(first.ToString("MMMM yyyy", culture));
        report.Fields["InpsCompanyNumber"] = data.Settings?.InpsCompanyNumber ?? string.Empty;
        report.Fields["InailCode"] = data.Settings?.InailCode ?? string.Empty;
        report.Fields["PrintDate"] = ReportFormat.Date(DateTime.Today, culture);

        foreach (var payslip in data.Payslips.OrderBy(static p => p.EmployeeName, StringComparer.CurrentCulture))
        {
            var employee = data.Employee(payslip.EmployeeId);
            report.AddRow("Payslips", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Pay_Employee"] = payslip.EmployeeName,
                ["Pay_TaxCode"] = employee?.SocialSecurityNumber ?? string.Empty,
                ["Pay_Contract"] = $"{employee?.CollectiveAgreementCode} {employee?.ContractLevel}".Trim(),
                ["Pay_Days"] = ReportFormat.Quantity(payslip.PaidDays, culture),
                ["Pay_Gross"] = ReportFormat.Amount(payslip.GrossSalary, culture),
                ["Pay_Inps"] = ReportFormat.Amount(data.Deducted(ItalianWageCodes.InpsEmployee, payslip.EmployeeId), culture),
                ["Pay_Irpef"] = ReportFormat.Amount(data.Deducted(ItalianWageCodes.Irpef, payslip.EmployeeId), culture),
                ["Pay_Net"] = ReportFormat.Amount(payslip.PayoutAmount, culture),
                ["Pay_EmployerInps"] = ReportFormat.Amount(data.Added(ItalianWageCodes.InpsEmployer, payslip.EmployeeId), culture),
                ["Pay_Tfr"] = ReportFormat.Amount(data.Added(ItalianWageCodes.Tfr, payslip.EmployeeId), culture),
            });

            foreach (var line in data.Lines.Where(l => l.PayslipId == payslip.Id).OrderBy(static l => l.SortOrder))
            {
                report.AddRow("Lines", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Line_Employee"] = payslip.EmployeeName,
                    ["Line_Code"] = line.WageTypeCode,
                    ["Line_Description"] = line.Description,
                    ["Line_Quantity"] = line.Quantity == 0m ? string.Empty : ReportFormat.Amount(line.Quantity, culture),
                    ["Line_Rate"] = line.Rate == 0m ? string.Empty : ReportFormat.Amount(line.Rate, culture),
                    ["Line_Amount"] = ReportFormat.Amount(line.Amount, culture),
                });
            }

            var attendance = await calculator.EvaluateAsync(payslip.EmployeeId, first, last, cancellationToken).ConfigureAwait(false);

            foreach (var day in attendance?.Days ?? [])
            {
                report.AddRow("Attendance", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Att_Employee"] = payslip.EmployeeName,
                    ["Att_Date"] = day.Date.ToString("ddd dd", culture),
                    ["Att_Worked"] = WorkingTimeReport.FormatMinutes(day.WorkedMinutes),
                    ["Att_Overtime"] = day.BalanceMinutes > 0 ? WorkingTimeReport.FormatMinutes(day.BalanceMinutes) : string.Empty,
                    ["Att_Absence"] = day.Absence ?? string.Empty,
                });
            }
        }

        return report;
    }
}
