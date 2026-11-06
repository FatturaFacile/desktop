// <copyright file="ItalianPayrollPeriodData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Payroll.Italian;

/// <summary>
/// Payslips, lines and employees of a period, used by the Italian filings (F24, UniEmens, CU, LUL).
/// </summary>
public sealed class ItalianPayrollPeriodData
{
    private ItalianPayrollPeriodData(IReadOnlyList<Payslip> payslips, IReadOnlyList<PayslipLine> lines, IReadOnlyList<HrEmployee> employees, ItalianPayrollSettings? settings)
    {
        Payslips = payslips;
        Lines = lines;
        Employees = employees;
        Settings = settings;
    }

    /// <summary>
    /// Gets the payslips.
    /// </summary>
    public IReadOnlyList<Payslip> Payslips { get; }

    /// <summary>
    /// Gets the payslip lines.
    /// </summary>
    public IReadOnlyList<PayslipLine> Lines { get; }

    /// <summary>
    /// Gets the employees.
    /// </summary>
    public IReadOnlyList<HrEmployee> Employees { get; }

    /// <summary>
    /// Gets the company settings.
    /// </summary>
    public ItalianPayrollSettings? Settings { get; }

    /// <summary>
    /// Loads the data of a year, or of one month when given.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="year">The year.</param>
    /// <param name="month">The month, or <see langword="null"/> for the whole year.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data.</returns>
    /// <exception cref="InvalidOperationException">There is no calculated payroll in the period.</exception>
    public static Task<ItalianPayrollPeriodData> LoadAsync(IUnitOfWork unitOfWork, int year, int? month, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);

        return LoadInternalAsync(unitOfWork, year, month, cancellationToken);
    }

    /// <summary>
    /// Sums the lines with a code, optionally for one employee.
    /// </summary>
    /// <param name="code">The line code.</param>
    /// <param name="employeeId">The employee, or <see langword="null"/> for everybody.</param>
    /// <returns>The total.</returns>
    public decimal Added(string code, Guid? employeeId = null)
    {
        var slipIds = Payslips.Where(p => employeeId == null || p.EmployeeId == employeeId).Select(static p => p.Id).ToHashSet();
        return Lines.Where(l => slipIds.Contains(l.PayslipId) && string.Equals(l.WageTypeCode, code, StringComparison.Ordinal)).Sum(static l => l.Amount);
    }

    /// <summary>
    /// Sums the deduction lines with a code as positive amounts (a refund is negative).
    /// </summary>
    /// <param name="code">The line code.</param>
    /// <param name="employeeId">The employee, or <see langword="null"/> for everybody.</param>
    /// <returns>The total withheld.</returns>
    public decimal Deducted(string code, Guid? employeeId = null) => -Added(code, employeeId);

    /// <summary>
    /// Gets the employee of a payslip.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <returns>The employee, if found.</returns>
    public HrEmployee? Employee(Guid employeeId) => Employees.FirstOrDefault(e => e.Id == employeeId);

    private static async Task<ItalianPayrollPeriodData> LoadInternalAsync(IUnitOfWork unitOfWork, int year, int? month, CancellationToken cancellationToken)
    {
        var payslips = await unitOfWork.Repository<Payslip>()
            .FindAsync(p => p.Year == year && (month == null || p.Month == month), cancellationToken)
            .ConfigureAwait(false);

        if (payslips.Count == 0)
        {
            throw new InvalidOperationException("Nessuna busta paga elaborata nel periodo.");
        }

        var ids = payslips.Select(static p => p.Id).ToHashSet();
        var lines = await unitOfWork.Repository<PayslipLine>().FindAsync(l => ids.Contains(l.PayslipId), cancellationToken).ConfigureAwait(false);
        var employeeIds = payslips.Select(static p => p.EmployeeId).ToHashSet();
        var employees = await unitOfWork.Repository<HrEmployee>().FindAsync(e => employeeIds.Contains(e.Id), cancellationToken).ConfigureAwait(false);
        var settings = (await unitOfWork.Repository<ItalianPayrollSettings>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false)).FirstOrDefault();
        return new ItalianPayrollPeriodData(payslips, lines, employees, settings);
    }
}
