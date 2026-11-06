// <copyright file="WorkingTimeCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.TimeTracking;

/// <summary>
/// Loads the working time data of an employee and evaluates it with <see cref="SwissWorkingTimeEngine"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="WorkingTimeCalculator"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class WorkingTimeCalculator(IUnitOfWork unitOfWork)
{
    /// <summary>
    /// Evaluates the working time of an employee for a period.
    /// </summary>
    /// <param name="employeeId">The employee.</param>
    /// <param name="from">The first day.</param>
    /// <param name="to">The last day.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The report, or null when the employee does not exist.</returns>
    public async Task<WorkingTimeReport?> EvaluateAsync(Guid employeeId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var employee = await unitOfWork.Repository<HrEmployee>().GetByIdAsync(employeeId, cancellationToken).ConfigureAwait(false);

        if (employee is null)
        {
            return null;
        }

        var yearStart = new DateTime(from.Year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var context = await LoadAsync(employee, yearStart, to.Date, cancellationToken).ConfigureAwait(false);

        var before = 0;

        if (from.Date > yearStart)
        {
            context.From = yearStart;
            context.To = from.Date.AddDays(-1);
            before = SwissWorkingTimeEngine.Evaluate(context).StatutoryOvertimeMinutes;
        }

        context.From = from.Date;
        context.To = to.Date;
        context.StatutoryOvertimeBeforeMinutes = before;
        return SwissWorkingTimeEngine.Evaluate(context);
    }

    private async Task<WorkingTimeContext> LoadAsync(HrEmployee employee, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var start = from.AddDays(-1);
        var end = to.AddDays(2);
        var employeeId = employee.Id;

        var events = await unitOfWork.Repository<TimeClockEvent>()
            .FindAsync(e => e.EmployeeId == employeeId && e.Timestamp >= start && e.Timestamp < end && e.DeletedAt == null, cancellationToken)
            .ConfigureAwait(false);
        var records = await unitOfWork.Repository<TimeDailyRecord>()
            .FindAsync(r => r.EmployeeId == employeeId && r.Date >= from && r.Date <= to, cancellationToken)
            .ConfigureAwait(false);
        var absences = await unitOfWork.Repository<HrAbsence>()
            .FindAsync(a => a.EmployeeId == employeeId && a.EndDate >= from && a.StartDate <= to, cancellationToken)
            .ConfigureAwait(false);
        var holidays = await unitOfWork.Repository<PublicHoliday>()
            .FindAsync(h => h.Date >= from && h.Date <= to, cancellationToken)
            .ConfigureAwait(false);

        return new WorkingTimeContext
        {
            Employee = employee,
            Events = events,
            DailyRecords = records,
            Absences = absences,
            Holidays = holidays,
        };
    }
}
