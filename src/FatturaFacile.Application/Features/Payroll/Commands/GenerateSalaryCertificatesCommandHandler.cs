// <copyright file="GenerateSalaryCertificatesCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Payroll.Swiss;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Handler for <see cref="GenerateSalaryCertificatesCommand"/>: sums the payslip lines of the closed runs by certificate box.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GenerateSalaryCertificatesCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class GenerateSalaryCertificatesCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<GenerateSalaryCertificatesCommand, int>
{
    private static readonly string[] _socialContributionCodes = [SwissWageTypeCodes.Ahv, SwissWageTypeCodes.Alv, SwissWageTypeCodes.Nbu];

    /// <summary>
    /// Fills the boxes of a certificate from the payslip lines of the year.
    /// </summary>
    /// <param name="certificate">The certificate.</param>
    /// <param name="lines">The lines of the closed payslips of the employee.</param>
    public static void Fill(SalaryCertificate certificate, IReadOnlyCollection<PayslipLine> lines)
    {
        ArgumentNullException.ThrowIfNull(certificate);
        ArgumentNullException.ThrowIfNull(lines);

        decimal Box(params string[] boxes) => lines
            .Where(l => WageTypeKind.IsGross(l.Kind) && boxes.Contains(l.CertificateBox, StringComparer.Ordinal))
            .Sum(static l => l.Amount);

        decimal Deducted(params string[] codes) => -lines
            .Where(l => string.Equals(l.Kind, WageTypeKind.Deduction, StringComparison.Ordinal) && codes.Contains(l.WageTypeCode, StringComparer.Ordinal))
            .Sum(static l => l.Amount);

        certificate.Box1Salary = Box("1");
        certificate.Box2Benefits = Box("2.1", "2.2", "2.3");
        certificate.Box3Irregular = Box("3");
        certificate.Box4Capital = Box("4");
        certificate.Box7Other = Box("5", "6", "7");
        certificate.Box8Gross = certificate.Box1Salary + certificate.Box2Benefits + certificate.Box3Irregular + certificate.Box4Capital + certificate.Box7Other;
        certificate.Box9SocialContributions = Deducted(_socialContributionCodes);
        certificate.Box10Pension = Deducted(SwissWageTypeCodes.Bvg);
        certificate.Box11Net = certificate.Box8Gross - certificate.Box9SocialContributions - certificate.Box10Pension;
        certificate.Box12WithholdingTax = Deducted(SwissWageTypeCodes.WithholdingTax);
        certificate.Box13Expenses = lines.Where(static l => string.Equals(l.Kind, WageTypeKind.Expense, StringComparison.Ordinal)).Sum(static l => l.Amount);
    }

    /// <inheritdoc/>
    public Task<int> Handle(GenerateSalaryCertificatesCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Year, cancellationToken);
    }

    private async Task<int> HandleInternalAsync(int year, CancellationToken cancellationToken)
    {
        var runs = await unitOfWork.Repository<PayrollRun>().FindAsync(r => r.Year == year, cancellationToken).ConfigureAwait(false);
        var runIds = runs.Where(static r => string.Equals(r.Status, "Final", StringComparison.Ordinal)).Select(static r => r.Id).ToHashSet();

        if (runIds.Count == 0)
        {
            throw new InvalidOperationException(string.Create(CultureInfo.InvariantCulture, $"Nessuna elaborazione chiusa nel {year}."));
        }

        var payslips = await unitOfWork.Repository<Payslip>().FindAsync(p => runIds.Contains(p.PayrollRunId), cancellationToken).ConfigureAwait(false);
        var payslipIds = payslips.Select(static p => p.Id).ToHashSet();
        var lines = await unitOfWork.Repository<PayslipLine>().FindAsync(l => payslipIds.Contains(l.PayslipId), cancellationToken).ConfigureAwait(false);
        var employees = (await unitOfWork.Repository<HrEmployee>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false)).ToDictionary(static e => e.Id);
        var existing = await unitOfWork.Repository<SalaryCertificate>().FindAsync(c => c.Year == year, cancellationToken).ConfigureAwait(false);

        foreach (var old in existing)
        {
            unitOfWork.Repository<SalaryCertificate>().Delete(old);
        }

        var count = 0;

        foreach (var group in payslips.GroupBy(static p => p.EmployeeId))
        {
            var ids = group.Select(static p => p.Id).ToHashSet();
            _ = employees.TryGetValue(group.Key, out var employee);
            var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
            var yearEnd = new DateTime(year, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);

            SalaryCertificate certificate = new()
            {
                EmployeeId = group.Key,
                Year = year,
                PeriodFrom = employee is not null && employee.HireDate > yearStart ? employee.HireDate : yearStart,
                PeriodTo = employee?.TerminationDate is { } exit && exit < yearEnd ? exit : yearEnd,
                GeneratedAt = DateTime.UtcNow,
            };

            Fill(certificate, [.. lines.Where(l => ids.Contains(l.PayslipId))]);
            await unitOfWork.Repository<SalaryCertificate>().AddAsync(certificate, cancellationToken).ConfigureAwait(false);
            count++;
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return count;
    }
}
