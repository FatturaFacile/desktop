// <copyright file="GetCuDataQueryHandler.cs" company="Fattura Facile">
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
/// Handles <see cref="GetCuDataQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class GetCuDataQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCuDataQuery, GeneratedFile>
{
    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    /// <inheritdoc />
    public Task<GeneratedFile> Handle(GetCuDataQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Year, cancellationToken);
    }

    private static byte[] Encode(CsvBuilder csv) => [.. Utf8WithBom.GetPreamble(), .. Utf8WithBom.GetBytes(csv.ToString())];

    private async Task<GeneratedFile> HandleInternalAsync(int year, CancellationToken cancellationToken)
    {
        var data = await ItalianPayrollPeriodData.LoadAsync(unitOfWork, year, month: null, cancellationToken).ConfigureAwait(false);
        CsvBuilder csv = new();
        csv.Row("Codice fiscale", "Cognome", "Nome", "Periodo dal", "Periodo al", "Punto 1 - Redditi di lavoro dipendente", "Punto 21 - Ritenute IRPEF", "Punto 22 - Addizionale regionale", "Addizionale comunale (saldo anno precedente)", "Somme cuneo fiscale non imponibili", "Contributi previdenziali a carico del lavoratore", "TFR maturato");

        foreach (var group in data.Payslips.GroupBy(static p => p.EmployeeId))
        {
            var employee = data.Employee(group.Key);
            var first = group.Min(static p => p.Month);
            var last = group.Max(static p => p.Month);
            csv.Row(
                employee?.SocialSecurityNumber,
                employee?.LastName,
                employee?.FirstName,
                new DateTime(year, first, 1, 0, 0, 0, DateTimeKind.Unspecified),
                new DateTime(year, last, 1, 0, 0, 0, DateTimeKind.Unspecified).AddMonths(1).AddDays(-1),
                group.Sum(static p => p.WithholdingTaxBasis),
                data.Deducted(ItalianWageCodes.Irpef, group.Key),
                data.Deducted(ItalianWageCodes.RegionalSurcharge, group.Key),
                data.Deducted(ItalianWageCodes.MunicipalSurcharge, group.Key),
                data.Added(ItalianWageCodes.TaxWedge, group.Key),
                data.Deducted(ItalianWageCodes.InpsEmployee, group.Key),
                data.Added(ItalianWageCodes.Tfr, group.Key));
        }

        return new GeneratedFile(
            string.Create(CultureInfo.InvariantCulture, $"CU_dati_{year}.csv"),
            Encode(csv),
            "Dati riepilogativi per la Certificazione Unica: riportarli nel software di compilazione e invio della CU (tracciato telematico dell'Agenzia delle Entrate) e verificarli con il controllo ufficiale.");
    }
}
