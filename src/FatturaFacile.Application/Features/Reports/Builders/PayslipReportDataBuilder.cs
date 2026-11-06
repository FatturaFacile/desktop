// <copyright file="PayslipReportDataBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Application.Payroll.Swiss;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Features.Reports.Builders;

/// <summary>
/// Builds the merge fields of a payslip: company, employee, period, bases, the "Lines" group (earnings and
/// deductions) and the "EmployerLines" group.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PayslipReportDataBuilder"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class PayslipReportDataBuilder(IUnitOfWork unitOfWork) : IReportDataBuilder
{
    /// <inheritdoc/>
    public ReportDocumentType DocumentType => ReportDocumentType.Payslip;

    /// <inheritdoc/>
    public Task<ReportData?> BuildAsync(Guid entityId, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(culture);

        return BuildInternalAsync(entityId, culture, cancellationToken);
    }

    /// <summary>
    /// Adds the company and employee fields shared by payslips and salary certificates.
    /// </summary>
    /// <param name="data">The report data.</param>
    /// <param name="tenant">The company.</param>
    /// <param name="employee">The employee.</param>
    /// <param name="culture">The culture.</param>
    internal static void AddCompanyAndEmployee(ReportData data, Tenant? tenant, HrEmployee? employee, CultureInfo culture)
    {
        var country = tenant?.Country ?? "CH";
        data.Fields["Company_Name"] = tenant?.LegalName is { Length: > 0 } legal ? legal : tenant?.Name ?? string.Empty;
        data.Fields["Company_AddressLine"] = ReportFormat.StreetLine(tenant?.Street, tenant?.HouseNumber);
        data.Fields["Company_CityLine"] = ReportFormat.CityLine(tenant?.PostalCode, tenant?.City, tenant?.Province, country, country);
        data.Fields["Company_UidNumber"] = tenant?.UidNumber ?? tenant?.VatNumber ?? string.Empty;
        data.Fields["Employee_Name"] = employee?.FullName ?? string.Empty;
        data.Fields["Employee_Number"] = employee?.EmployeeNumber ?? string.Empty;
        data.Fields["Employee_AddressLine"] = ReportFormat.StreetLine(employee?.Street, employee?.HouseNumber);
        data.Fields["Employee_CityLine"] = ReportFormat.CityLine(employee?.PostalCode, employee?.City, province: null, employee?.ResidenceCountry, country);
        data.Fields["Employee_AhvNumber"] = employee?.SocialSecurityNumber ?? string.Empty;
        data.Fields["Employee_DateOfBirth"] = ReportFormat.Date(employee?.DateOfBirth, culture);
        data.Fields["Employee_HireDate"] = ReportFormat.Date(employee?.HireDate, culture);
        data.Fields["Employee_TerminationDate"] = ReportFormat.Date(employee?.TerminationDate, culture);
        data.Fields["Employee_JobTitle"] = employee?.JobTitle ?? string.Empty;
        data.Fields["Employee_Iban"] = employee?.Iban ?? string.Empty;
        data.Fields["Employee_Permit"] = employee?.ResidencePermit ?? string.Empty;
    }

    private static void AddLines(ReportData data, IEnumerable<PayslipLine> lines, CultureInfo culture)
    {
        foreach (var line in lines.OrderBy(static l => l.SortOrder))
        {
            var group = string.Equals(line.Kind, WageTypeKind.Employer, StringComparison.Ordinal) ? "EmployerLines" : "Lines";
            var percent = WageTypeKind.IsContribution(line.Kind);
            data.AddRow(group, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Line_Code"] = line.WageTypeCode,
                ["Line_Description"] = line.Description,
                ["Line_Quantity"] = FormatQuantity(line, percent, culture),
                ["Line_Rate"] = FormatRate(line, percent, culture),
                ["Line_Amount"] = ReportFormat.Amount(line.Amount, culture),
            });
        }
    }

    private static string FormatQuantity(PayslipLine line, bool percent, CultureInfo culture)
    {
        var hidden = line.Quantity == 0m || (line.Quantity == 1m && !percent);
        return hidden ? string.Empty : ReportFormat.Amount(line.Quantity, culture);
    }

    private static string FormatRate(PayslipLine line, bool percent, CultureInfo culture) => (line.Rate == 0m, percent) switch
    {
        (true, _) => string.Empty,
        (false, true) => ReportFormat.Percent(line.Rate, culture),
        _ => ReportFormat.Amount(line.Rate, culture),
    };

    private async Task<ReportData?> BuildInternalAsync(Guid entityId, CultureInfo culture, CancellationToken cancellationToken)
    {
        var payslip = await unitOfWork.Repository<Payslip>().GetByIdAsync(entityId, cancellationToken).ConfigureAwait(false);

        if (payslip is null)
        {
            return null;
        }

        var employee = await unitOfWork.Repository<HrEmployee>().GetByIdAsync(payslip.EmployeeId, cancellationToken).ConfigureAwait(false);
        var tenant = await unitOfWork.Repository<Tenant>().GetByIdAsync(payslip.TenantId, cancellationToken).ConfigureAwait(false);
        var run = await unitOfWork.Repository<PayrollRun>().GetByIdAsync(payslip.PayrollRunId, cancellationToken).ConfigureAwait(false);
        var lines = await unitOfWork.Repository<PayslipLine>().FindAsync(l => l.PayslipId == payslip.Id, cancellationToken).ConfigureAwait(false);
        var period = new DateTime(payslip.Year, payslip.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);

        ReportData data = new()
        {
            SuggestedFileName = ReportFormat.SafeFileName(string.Create(CultureInfo.InvariantCulture, $"Busta_paga_{payslip.Year}-{payslip.Month:00}_{payslip.EmployeeName}.pdf")),
        };

        AddCompanyAndEmployee(data, tenant, employee, culture);
        data.Fields["Period"] = culture.TextInfo.ToTitleCase(period.ToString("MMMM yyyy", culture));
        data.Fields["PaymentDate"] = ReportFormat.Date(run?.PaymentDate, culture);
        data.Fields["PaidDays"] = ReportFormat.Quantity(payslip.PaidDays, culture);
        data.Fields["Gross"] = ReportFormat.Amount(payslip.GrossSalary, culture);
        data.Fields["Deductions"] = ReportFormat.Amount(payslip.EmployeeDeductions, culture);
        data.Fields["Net"] = ReportFormat.Amount(payslip.NetSalary, culture);
        data.Fields["Payout"] = ReportFormat.Amount(payslip.PayoutAmount, culture);
        data.Fields["EmployerContributions"] = ReportFormat.Amount(payslip.EmployerContributions, culture);
        data.Fields["AhvBasis"] = ReportFormat.Amount(payslip.AhvBasis, culture);
        data.Fields["AlvBasis"] = ReportFormat.Amount(payslip.AlvBasis, culture);
        data.Fields["UvgBasis"] = ReportFormat.Amount(payslip.UvgBasis, culture);
        data.Fields["BvgInsuredSalary"] = ReportFormat.Amount(payslip.BvgInsuredSalary, culture);
        data.Fields["WithholdingTax"] = payslip.WithholdingTaxCode is null
            ? string.Empty
            : string.Create(culture, $"{payslip.WithholdingTaxCanton} {payslip.WithholdingTaxCode} {ReportFormat.Percent(payslip.WithholdingTaxRatePercent, culture)}");
        AddLines(data, lines, culture);
        return data;
    }
}
