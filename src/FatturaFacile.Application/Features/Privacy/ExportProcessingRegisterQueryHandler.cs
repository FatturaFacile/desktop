// <copyright file="ExportProcessingRegisterQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using FatturaFacile.Application.Common.Export;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Models;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Privacy;

/// <summary>
/// Handles <see cref="ExportProcessingRegisterQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentTenantService">The active company.</param>
public sealed class ExportProcessingRegisterQueryHandler(IUnitOfWork unitOfWork, ICurrentTenantService currentTenantService) : IRequestHandler<ExportProcessingRegisterQuery, GeneratedFile>
{
    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    /// <inheritdoc />
    public async Task<GeneratedFile> Handle(ExportProcessingRegisterQuery request, CancellationToken cancellationToken)
    {
        var activities = await unitOfWork.Repository<ProcessingActivity>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);
        CsvBuilder csv = new();
        csv.Row("Titolare", "Trattamento", "Finalità", "Base giuridica", "Interessati", "Categorie di dati", "Destinatari", "Trasferimenti all'estero", "Conservazione", "Misure di sicurezza", "Aggiornato il");

        foreach (var activity in activities.OrderBy(static a => a.Name, StringComparer.CurrentCulture))
        {
            csv.Row(currentTenantService.TenantName, activity.Name, activity.Purpose, activity.LegalBasis, activity.DataSubjects, activity.DataCategories, activity.Recipients, activity.ForeignTransfers, activity.RetentionPeriod, activity.SecurityMeasures, activity.UpdatedAt ?? activity.CreatedAt);
        }

        return new GeneratedFile(
            string.Create(CultureInfo.InvariantCulture, $"Registro_trattamenti_{DateTime.Today:yyyyMMdd}.csv"),
            Utf8WithBom.GetPreamble().Concat(Utf8WithBom.GetBytes(csv.ToString())).ToArray(),
            string.Create(CultureInfo.InvariantCulture, $"{activities.Count} trattamenti esportati."));
    }
}
