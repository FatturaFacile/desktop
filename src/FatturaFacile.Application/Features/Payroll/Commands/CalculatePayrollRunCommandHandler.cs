// <copyright file="CalculatePayrollRunCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Payroll.Swiss;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Handler for <see cref="CalculatePayrollRunCommand"/>: deletes the payslips of the draft run and calculates them again.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CalculatePayrollRunCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="tariffStore">The withholding tax tariffs.</param>
/// <param name="currentTenantService">The current company.</param>
public sealed class CalculatePayrollRunCommandHandler(
    IUnitOfWork unitOfWork,
    IWithholdingTaxTariffStore tariffStore,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<CalculatePayrollRunCommand, Guid>
{
    /// <inheritdoc/>
    public Task<Guid> Handle(CalculatePayrollRunCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return !string.Equals(currentTenantService.CountryCode, "CH", StringComparison.OrdinalIgnoreCase)
            ? throw new InvalidOperationException("L'elaborazione mensile Swissdec è disponibile per le aziende svizzere.")
            : HandleInternalAsync(request, cancellationToken);
    }

    private static SwissPayrollYearToDate YearToDate(IEnumerable<Payslip> previous, Guid employeeId)
    {
        var slips = previous.Where(p => p.EmployeeId == employeeId).ToList();
        return new SwissPayrollYearToDate(slips.Sum(static p => p.PaidDays), slips.Sum(static p => p.AlvBasis), slips.Sum(static p => p.UvgBasis));
    }

    private async Task<Guid> HandleInternalAsync(CalculatePayrollRunCommand request, CancellationToken cancellationToken)
    {
        var run = await PrepareRunAsync(request, cancellationToken).ConfigureAwait(false);
        var first = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var last = first.AddMonths(1).AddDays(-1);

        var settings = (await unitOfWork.Repository<SwissPayrollSettings>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false)).FirstOrDefault()
            ?? throw new InvalidOperationException("Impostazioni paghe mancanti: aprire Paghe > Impostazioni.");
        var wageTypes = (await unitOfWork.Repository<PayrollWageType>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false))
            .ToDictionary(static w => w.Code, StringComparer.Ordinal);
        var employees = await unitOfWork.Repository<HrEmployee>().FindAsync(
            e => e.HireDate <= last && (e.TerminationDate == null || e.TerminationDate >= first) && e.DeletedAt == null,
            cancellationToken).ConfigureAwait(false);
        var inputs = await unitOfWork.Repository<PayrollInput>().FindAsync(i => i.Year == request.Year && i.Month == request.Month, cancellationToken).ConfigureAwait(false);
        var absences = await unitOfWork.Repository<HrAbsence>().FindAsync(a => a.StartDate <= last && a.EndDate >= first, cancellationToken).ConfigureAwait(false);
        var children = await unitOfWork.Repository<HrEmployeeChild>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);
        var allowanceRates = await unitOfWork.Repository<FamilyAllowanceRate>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);
        var previous = await unitOfWork.Repository<Payslip>().FindAsync(p => p.Year == request.Year && p.Month < request.Month, cancellationToken).ConfigureAwait(false);

        foreach (var employee in employees)
        {
            SwissPayrollContext context = new()
            {
                Employee = employee,
                Settings = settings,
                Year = request.Year,
                Month = request.Month,
                WageTypes = wageTypes,
                Inputs = [.. inputs.Where(i => i.EmployeeId == employee.Id)],
                Absences = [.. absences.Where(a => a.EmployeeId == employee.Id)],
                Children = [.. children.Where(c => c.EmployeeId == employee.Id)],
                FamilyAllowanceRates = allowanceRates,
                WithholdingTaxTariffs = await LoadTariffsAsync(employee, settings, cancellationToken).ConfigureAwait(false),
                YearToDate = YearToDate(previous, employee.Id),
            };

            var result = SwissPayrollEngine.Calculate(context);

            if (result is null)
            {
                continue;
            }

            result.Payslip.PayrollRunId = run.Id;
            await unitOfWork.Repository<Payslip>().AddAsync(result.Payslip, cancellationToken).ConfigureAwait(false);

            foreach (var line in result.Lines)
            {
                line.PayslipId = result.Payslip.Id;
                await unitOfWork.Repository<PayslipLine>().AddAsync(line, cancellationToken).ConfigureAwait(false);
            }
        }

        run.CalculatedAt = DateTime.UtcNow;
        unitOfWork.Repository<PayrollRun>().Update(run);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return run.Id;
    }

    private async Task<PayrollRun> PrepareRunAsync(CalculatePayrollRunCommand request, CancellationToken cancellationToken)
    {
        var runs = unitOfWork.Repository<PayrollRun>();
        var run = (await runs.FindAsync(r => r.Year == request.Year && r.Month == request.Month, cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        if (run is null)
        {
            run = new PayrollRun { Year = request.Year, Month = request.Month, PaymentDate = request.PaymentDate };
            await runs.AddAsync(run, cancellationToken).ConfigureAwait(false);
            _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return run;
        }

        if (string.Equals(run.Status, "Final", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(string.Create(CultureInfo.InvariantCulture, $"L'elaborazione {request.Month:00}/{request.Year} è chiusa: riaprirla per ricalcolarla."));
        }

        run.PaymentDate = request.PaymentDate ?? run.PaymentDate;
        var payslips = await unitOfWork.Repository<Payslip>().FindAsync(p => p.PayrollRunId == run.Id, cancellationToken).ConfigureAwait(false);
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

    private async Task<IReadOnlyList<WithholdingTaxTariff>> LoadTariffsAsync(HrEmployee employee, SwissPayrollSettings settings, CancellationToken cancellationToken)
    {
        if (!employee.IsWithholdingTaxSubject || string.IsNullOrWhiteSpace(employee.WithholdingTaxCode))
        {
            return [];
        }

        var resident = string.Equals(employee.ResidenceCountry ?? "CH", "CH", StringComparison.OrdinalIgnoreCase);
        var canton = (resident ? employee.ResidenceCanton ?? employee.WorkCanton : employee.WorkCanton) ?? settings.DefaultWorkCanton;
        return await tariffStore.GetAsync(canton, employee.WithholdingTaxCode, cancellationToken).ConfigureAwait(false);
    }
}
