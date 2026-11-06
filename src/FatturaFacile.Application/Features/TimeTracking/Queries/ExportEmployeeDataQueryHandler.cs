// <copyright file="ExportEmployeeDataQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Queries;

/// <summary>
/// Handles <see cref="ExportEmployeeDataQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="access">The time recording access rules.</param>
/// <param name="calculator">The working time evaluation.</param>
/// <param name="currentUserService">The signed-in user.</param>
/// <param name="currentTenantService">The active company.</param>
public sealed class ExportEmployeeDataQueryHandler(
    IUnitOfWork unitOfWork,
    TimeAccess access,
    WorkingTimeCalculator calculator,
    ICurrentUserService currentUserService,
    ICurrentTenantService currentTenantService) : IRequestHandler<ExportEmployeeDataQuery, EmployeeDataExport>
{
    /// <inheritdoc />
    public Task<EmployeeDataExport> Handle(ExportEmployeeDataQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.EmployeeId, cancellationToken);
    }

    private static string FileName(HrEmployee employee)
    {
        var name = string.Concat($"{employee.LastName}-{employee.FirstName}".Where(static c => char.IsLetterOrDigit(c) || c == '-'));
        return string.Create(CultureInfo.InvariantCulture, $"dati-personali-{name}-{DateTime.Today:yyyyMMdd}.zip");
    }

    private async Task<EmployeeDataExport> HandleInternalAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        await access.EnsureCanReadAsync(employeeId, cancellationToken).ConfigureAwait(false);

        var employee = await unitOfWork.Repository<HrEmployee>().GetByIdAsync(employeeId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Dipendente non trovato.");
        var own = await access.CurrentEmployeeAsync(cancellationToken).ConfigureAwait(false);
        var includesPayroll = own?.Id == employeeId || currentUserService.HasPermission(currentTenantService.TenantId, Permission.PayrollRead);

        var clockEvents = await unitOfWork.Repository<TimeClockEvent>().FindAsync(e => e.EmployeeId == employeeId, cancellationToken).ConfigureAwait(false);
        var dailyRecords = await unitOfWork.Repository<TimeDailyRecord>().FindAsync(r => r.EmployeeId == employeeId, cancellationToken).ConfigureAwait(false);
        var first = clockEvents.Select(static e => e.Timestamp.Date).Concat(dailyRecords.Select(static r => r.Date.Date)).DefaultIfEmpty(DateTime.Today).Min();

        var payslips = includesPayroll
            ? await unitOfWork.Repository<Payslip>().FindAsync(p => p.EmployeeId == employeeId, cancellationToken).ConfigureAwait(false)
            : [];
        var payslipIds = payslips.Select(static p => p.Id).ToList();

        EmployeeDataContent content = new()
        {
            Employee = employee,
            CompanyName = currentTenantService.TenantName,
            Children = await unitOfWork.Repository<HrEmployeeChild>().FindAsync(c => c.EmployeeId == employeeId, cancellationToken).ConfigureAwait(false),
            Absences = await unitOfWork.Repository<HrAbsence>().FindAsync(a => a.EmployeeId == employeeId, cancellationToken).ConfigureAwait(false),
            ClockEvents = clockEvents,
            DailyRecords = dailyRecords,
            Audit = await unitOfWork.Repository<TimeAuditEntry>().FindAsync(a => a.EmployeeId == employeeId, cancellationToken).ConfigureAwait(false),
            Report = await calculator.EvaluateAsync(employeeId, first, DateTime.Today, cancellationToken).ConfigureAwait(false),
            IncludesPayroll = includesPayroll,
            Payslips = payslips,
            PayslipLines = payslipIds.Count == 0
                ? []
                : await unitOfWork.Repository<PayslipLine>().FindAsync(l => payslipIds.Contains(l.PayslipId), cancellationToken).ConfigureAwait(false),
            PayrollInputs = includesPayroll
                ? await unitOfWork.Repository<PayrollInput>().FindAsync(i => i.EmployeeId == employeeId, cancellationToken).ConfigureAwait(false)
                : [],
            SalaryCertificates = includesPayroll
                ? await unitOfWork.Repository<SalaryCertificate>().FindAsync(c => c.EmployeeId == employeeId, cancellationToken).ConfigureAwait(false)
                : [],
        };

        return new EmployeeDataExport(FileName(employee), EmployeeDataArchive.Build(content));
    }
}
