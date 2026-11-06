// <copyright file="GetPayrollF24QueryHandler.cs" company="Fattura Facile">
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
/// Handles <see cref="GetPayrollF24Query"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class GetPayrollF24QueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetPayrollF24Query, GeneratedFile>
{
    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    /// <inheritdoc />
    public Task<GeneratedFile> Handle(GetPayrollF24Query request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Year, request.Month, cancellationToken);
    }

    private static byte[] Encode(CsvBuilder csv) => [.. Utf8WithBom.GetPreamble(), .. Utf8WithBom.GetBytes(csv.ToString())];

    private async Task<GeneratedFile> HandleInternalAsync(int year, int month, CancellationToken cancellationToken)
    {
        var data = await ItalianPayrollPeriodData.LoadAsync(unitOfWork, year, month, cancellationToken).ConfigureAwait(false);
        var period = string.Create(CultureInfo.InvariantCulture, $"{month:00}{year}");
        var due = new DateTime(year, month, 16, 0, 0, 0, DateTimeKind.Unspecified).AddMonths(1);
        CsvBuilder csv = new();
        csv.Row("Sezione", "Codice tributo / causale", "Codice ente / sede / regione", "Matricola / rateazione", "Periodo / anno di riferimento", "Importo a debito", "Importo a credito");
        csv.Row("Erario", "1001", string.Empty, string.Empty, year, data.Deducted(ItalianWageCodes.Irpef), 0m);

        foreach (var region in data.Payslips.GroupBy(p => data.Employee(p.EmployeeId)?.RegionCode ?? string.Empty, StringComparer.Ordinal))
        {
            var amount = region.Sum(p => data.Deducted(ItalianWageCodes.RegionalSurcharge, p.EmployeeId));

            if (amount > 0m)
            {
                csv.Row("Regioni", "3802", region.Key, string.Create(CultureInfo.InvariantCulture, $"{month:00}11"), year - 1, amount, 0m);
            }
        }

        foreach (var municipality in data.Payslips.GroupBy(p => data.Employee(p.EmployeeId)?.MunicipalityCode ?? string.Empty, StringComparer.Ordinal))
        {
            var amount = municipality.Sum(p => data.Deducted(ItalianWageCodes.MunicipalSurcharge, p.EmployeeId));

            if (amount > 0m)
            {
                csv.Row("IMU e altri tributi locali", "3848", municipality.Key, string.Create(CultureInfo.InvariantCulture, $"{month:00}11"), year - 1, amount, 0m);
            }
        }

        var inps = data.Deducted(ItalianWageCodes.InpsEmployee) + data.Added(ItalianWageCodes.InpsEmployer);
        csv.Row("INPS", "DM10", data.Settings?.InpsOfficeCode ?? string.Empty, data.Settings?.InpsCompanyNumber ?? string.Empty, period, inps, 0m);
        var wedge = data.Added(ItalianWageCodes.TaxWedge);
        var summary = string.Create(
            CultureInfo.InvariantCulture,
            $"F24 da versare entro il {due:dd.MM.yyyy}: IRPEF {data.Deducted(ItalianWageCodes.Irpef):0.00}, INPS {inps:0.00}. Somme cuneo fiscale anticipate ai dipendenti {wedge:0.00}: recuperarle in compensazione con il codice tributo indicato dall'Agenzia delle Entrate.");
        return new GeneratedFile(string.Create(CultureInfo.InvariantCulture, $"F24_paghe_{year}-{month:00}.csv"), Encode(csv), summary);
    }
}
