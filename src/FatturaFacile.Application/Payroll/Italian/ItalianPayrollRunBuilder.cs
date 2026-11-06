// <copyright file="ItalianPayrollRunBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Payroll.Italian;

/// <summary>
/// Calculates the Italian monthly payroll run of the active company.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class ItalianPayrollRunBuilder(IUnitOfWork unitOfWork)
{
    /// <summary>
    /// Calculates (or recalculates) the run of a month.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="month">The month.</param>
    /// <param name="paymentDate">The payment date.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The run identifier.</returns>
    public async Task<Guid> CalculateAsync(int year, int month, DateTime? paymentDate, CancellationToken cancellationToken = default)
    {
        var run = await PrepareRunAsync(year, month, paymentDate, cancellationToken).ConfigureAwait(false);
        var first = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var last = first.AddMonths(1).AddDays(-1);
        var settings = (await unitOfWork.Repository<ItalianPayrollSettings>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false)).FirstOrDefault();
        var agreements = await unitOfWork.Repository<CollectiveAgreement>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);
        var levels = await unitOfWork.Repository<CollectiveAgreementLevel>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);
        var employees = await unitOfWork.Repository<HrEmployee>().FindAsync(
            e => e.HireDate <= last && (e.TerminationDate == null || e.TerminationDate >= first) && e.DeletedAt == null,
            cancellationToken).ConfigureAwait(false);
        var inputs = await unitOfWork.Repository<PayrollInput>().FindAsync(i => i.Year == year && i.Month == month, cancellationToken).ConfigureAwait(false);
        var absences = await unitOfWork.Repository<HrAbsence>().FindAsync(a => a.StartDate <= last && a.EndDate >= first, cancellationToken).ConfigureAwait(false);
        var previous = await unitOfWork.Repository<Payslip>().FindAsync(p => p.Year == year && p.Month < month, cancellationToken).ConfigureAwait(false);
        var previousIds = previous.Select(static p => p.Id).ToHashSet();
        var previousLines = await unitOfWork.Repository<PayslipLine>().FindAsync(l => previousIds.Contains(l.PayslipId), cancellationToken).ConfigureAwait(false);
        var lastYear = await unitOfWork.Repository<Payslip>().FindAsync(p => p.Year == year - 1, cancellationToken).ConfigureAwait(false);

        foreach (var employee in employees)
        {
            var agreement = agreements.FirstOrDefault(a => string.Equals(a.Code, employee.CollectiveAgreementCode, StringComparison.OrdinalIgnoreCase));
            var employeeSlips = previous.Where(p => p.EmployeeId == employee.Id).ToList();
            var slipIds = employeeSlips.Select(static p => p.Id).ToHashSet();

            ItalianPayrollContext context = new()
            {
                Employee = employee,
                Year = year,
                Month = month,
                Settings = settings,
                Agreement = agreement,
                Level = agreement is null
                    ? null
                    : levels.FirstOrDefault(l => l.AgreementId == agreement.Id && string.Equals(l.Level, employee.ContractLevel, StringComparison.OrdinalIgnoreCase)),
                Inputs = [.. inputs.Where(i => i.EmployeeId == employee.Id)],
                Absences = [.. absences.Where(a => a.EmployeeId == employee.Id)],
                YearToDate = employeeSlips,
                YearToDateLines = [.. previousLines.Where(l => slipIds.Contains(l.PayslipId))],
                PreviousYearTaxable = lastYear.Where(p => p.EmployeeId == employee.Id).Sum(static p => p.WithholdingTaxBasis),
            };

            if (ItalianPayrollEngine.Calculate(context) is not { } result)
            {
                continue;
            }

            result.Payslip.PayrollRunId = run.Id;
            await unitOfWork.Repository<Payslip>().AddAsync(result.Payslip, cancellationToken).ConfigureAwait(false);

            foreach (var line in result.Lines)
            {
                await unitOfWork.Repository<PayslipLine>().AddAsync(line, cancellationToken).ConfigureAwait(false);
            }
        }

        run.CalculatedAt = DateTime.UtcNow;
        unitOfWork.Repository<PayrollRun>().Update(run);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return run.Id;
    }

    private async Task<PayrollRun> PrepareRunAsync(int year, int month, DateTime? paymentDate, CancellationToken cancellationToken)
    {
        var runs = unitOfWork.Repository<PayrollRun>();
        var run = (await runs.FindAsync(r => r.Year == year && r.Month == month, cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        if (run is null)
        {
            run = new PayrollRun { Year = year, Month = month, PaymentDate = paymentDate };
            await runs.AddAsync(run, cancellationToken).ConfigureAwait(false);
            _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return run;
        }

        if (string.Equals(run.Status, "Final", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(string.Create(CultureInfo.InvariantCulture, $"L'elaborazione {month:00}/{year} è chiusa: riaprirla per ricalcolarla."));
        }

        run.PaymentDate = paymentDate ?? run.PaymentDate;
        var runId = run.Id;
        var payslips = await unitOfWork.Repository<Payslip>().FindAsync(p => p.PayrollRunId == runId, cancellationToken).ConfigureAwait(false);
        var payslipIds = payslips.Select(static p => p.Id).ToHashSet();
        var lines = await unitOfWork.Repository<PayslipLine>().FindAsync(l => payslipIds.Contains(l.PayslipId), cancellationToken).ConfigureAwait(false);

        foreach (var line in lines)
        {
            unitOfWork.Repository<PayslipLine>().Delete(line);
        }

        foreach (var payslip in payslips)
        {
            unitOfWork.Repository<Payslip>().Delete(payslip);
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return run;
    }
}
