// <copyright file="ExportVatRegisterQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="ExportVatRegisterQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="renderer">The PDF renderer.</param>
/// <param name="localization">The localization service.</param>
/// <param name="tenant">The active tenant.</param>
public sealed class ExportVatRegisterQueryHandler(IUnitOfWork unitOfWork, ITabularPdfRenderer renderer, ILocalizationService localization, ICurrentTenantService tenant)
    : IRequestHandler<ExportVatRegisterQuery, GeneratedFile>
{
    /// <inheritdoc/>
    public Task<GeneratedFile> Handle(ExportVatRegisterQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<GeneratedFile> HandleInternalAsync(ExportVatRegisterQuery request, CancellationToken cancellationToken)
    {
        var kind = request.Kind;
        var from = request.From.Date;
        var to = request.To.Date;
        var entries = await unitOfWork.Repository<VatRegisterEntry>()
            .FindAsync(e => e.Kind == kind && e.RegistrationDate >= from && e.RegistrationDate <= to, cancellationToken)
            .ConfigureAwait(false);
        var report = VatRegisterReportBuilder.Build(kind, request.Section, from, to, request.Section is { } section ? entries.Where(e => e.Section == section) : entries);
        var name = string.Create(CultureInfo.InvariantCulture, $"vat-register-{kind}-{from:yyyyMMdd}-{to:yyyyMMdd}");
        var summary = string.Format(CultureInfo.CurrentCulture, localization["VatCompliance_Export_RegisterSummary"], report.Lines.Count);

        if (!request.Pdf)
        {
            return new GeneratedFile(name + ".csv", VatRegisterCsvBuilder.Build(report), summary);
        }

        var pdf = await renderer.RenderAsync(VatRegisterTable.Build(report, key => localization[key], tenant.TenantName), firstPage: 1, cancellationToken).ConfigureAwait(false);

        return new GeneratedFile(name + ".pdf", pdf, summary);
    }
}
