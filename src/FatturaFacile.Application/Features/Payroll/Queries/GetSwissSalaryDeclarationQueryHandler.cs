// <copyright file="GetSwissSalaryDeclarationQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using FatturaFacile.Application.Common.Export;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Models;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Queries;

/// <summary>
/// Handles <see cref="GetSwissSalaryDeclarationQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class GetSwissSalaryDeclarationQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetSwissSalaryDeclarationQuery, GeneratedFile>
{
    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    /// <inheritdoc />
    public Task<GeneratedFile> Handle(GetSwissSalaryDeclarationQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Year, cancellationToken);
    }

    private async Task<GeneratedFile> HandleInternalAsync(int year, CancellationToken cancellationToken)
    {
        var runs = await unitOfWork.Repository<PayrollRun>().FindAsync(r => r.Year == year, cancellationToken).ConfigureAwait(false);
        var closed = runs.Where(static r => string.Equals(r.Status, "Final", StringComparison.Ordinal)).Select(static r => r.Id).ToHashSet();
        var payslips = (await unitOfWork.Repository<Payslip>().FindAsync(p => p.Year == year, cancellationToken).ConfigureAwait(false))
            .Where(p => closed.Contains(p.PayrollRunId))
            .ToList();

        if (payslips.Count == 0)
        {
            throw new InvalidOperationException("Nessuna elaborazione chiusa nell'anno.");
        }

        var employeeIds = payslips.Select(static p => p.EmployeeId).ToHashSet();
        var employees = await unitOfWork.Repository<HrEmployee>().FindAsync(e => employeeIds.Contains(e.Id), cancellationToken).ConfigureAwait(false);
        CsvBuilder csv = new();
        csv.Row("N. AVS", "Cognome", "Nome", "Data di nascita", "Sesso", "Dal", "Al", "Salario lordo", "Salario AVS", "Salario AD", "Salario LAINF", "Salario IGM", "Salario assicurato LPP (media)", "Imposta alla fonte - salario determinante", "Cantone di lavoro");

        foreach (var group in payslips.GroupBy(static p => p.EmployeeId))
        {
            var employee = employees.FirstOrDefault(e => e.Id == group.Key);
            var from = new DateTime(year, group.Min(static p => p.Month), 1, 0, 0, 0, DateTimeKind.Unspecified);
            var to = new DateTime(year, group.Max(static p => p.Month), 1, 0, 0, 0, DateTimeKind.Unspecified).AddMonths(1).AddDays(-1);
            csv.Row(
                employee?.SocialSecurityNumber,
                employee?.LastName,
                employee?.FirstName,
                employee?.DateOfBirth,
                employee?.Gender,
                from,
                to,
                group.Sum(static p => p.GrossSalary),
                group.Sum(static p => p.AhvBasis),
                group.Sum(static p => p.AlvBasis),
                group.Sum(static p => p.UvgBasis),
                group.Sum(static p => p.KtgBasis),
                Math.Round(group.Average(static p => p.BvgInsuredSalary), 2, MidpointRounding.AwayFromZero),
                group.Sum(static p => p.WithholdingTaxBasis),
                employee?.WorkCanton);
        }

        csv.Row("Totale", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, payslips.Sum(static p => p.GrossSalary), payslips.Sum(static p => p.AhvBasis), payslips.Sum(static p => p.AlvBasis), payslips.Sum(static p => p.UvgBasis), payslips.Sum(static p => p.KtgBasis), string.Empty, payslips.Sum(static p => p.WithholdingTaxBasis), string.Empty);
        return new GeneratedFile(
            string.Create(CultureInfo.InvariantCulture, $"Dichiarazione_salari_{year}.csv"),
            Utf8WithBom.GetPreamble().Concat(Utf8WithBom.GetBytes(csv.ToString())).ToArray(),
            "Dichiarazione dei salari per cassa AVS, LAINF, IGM e LPP. Senza un distributore Swissdec certificato va inviata con i portali delle casse o allegata ai loro moduli.");
    }
}
