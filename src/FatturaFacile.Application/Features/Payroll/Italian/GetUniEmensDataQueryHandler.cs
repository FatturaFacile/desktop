// <copyright file="GetUniEmensDataQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using FatturaFacile.Application.Common.Export;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.Payroll.Italian;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Italian;

/// <summary>
/// Handles <see cref="GetUniEmensDataQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class GetUniEmensDataQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetUniEmensDataQuery, GeneratedFile>
{
    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    /// <inheritdoc />
    public Task<GeneratedFile> Handle(GetUniEmensDataQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Year, request.Month, cancellationToken);
    }

    private static byte[] Encode(CsvBuilder csv) => [.. Utf8WithBom.GetPreamble(), .. Utf8WithBom.GetBytes(csv.ToString())];

    private async Task<GeneratedFile> HandleInternalAsync(int year, int month, CancellationToken cancellationToken)
    {
        var data = await ItalianPayrollPeriodData.LoadAsync(unitOfWork, year, month, cancellationToken).ConfigureAwait(false);
        CsvBuilder csv = new();
        csv.Row("Matricola azienda", "Periodo", "Codice fiscale", "Cognome", "Nome", "Qualifica", "CCNL", "Livello", "Imponibile", "Contributo dipendente", "Contributo azienda", "Giorni retribuiti", "Settimane utili", "Data assunzione", "Data cessazione");

        foreach (var payslip in data.Payslips.OrderBy(static p => p.EmployeeName, StringComparer.CurrentCulture))
        {
            var employee = data.Employee(payslip.EmployeeId);
            csv.Row(
                data.Settings?.InpsCompanyNumber,
                string.Create(CultureInfo.InvariantCulture, $"{year}-{month:00}"),
                employee?.SocialSecurityNumber,
                employee?.LastName,
                employee?.FirstName,
                employee?.InpsQualification,
                employee?.CollectiveAgreementCode,
                employee?.ContractLevel,
                payslip.AhvBasis,
                data.Deducted(ItalianWageCodes.InpsEmployee, payslip.EmployeeId),
                data.Added(ItalianWageCodes.InpsEmployer, payslip.EmployeeId),
                payslip.PaidDays,
                Math.Ceiling(payslip.PaidDays / 6m),
                employee?.HireDate,
                employee?.TerminationDate);
        }

        return new GeneratedFile(
            string.Create(CultureInfo.InvariantCulture, $"UniEmens_dati_{year}-{month:00}.csv"),
            Encode(csv),
            "Dati per le denunce individuali UniEmens: caricarli nel software di invio (o nel gestionale del consulente) e controllarli prima della trasmissione all'INPS.");
    }
}
