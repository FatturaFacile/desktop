// <copyright file="ExportCuPdfQueryHandler.cs" company="Fattura Facile">
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
/// Handles <see cref="ExportCuPdfQuery"/>: renders the certificates and freezes them as issued.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="renderer">The PDF renderer.</param>
/// <param name="localization">The localization service.</param>
/// <param name="tenant">The active tenant.</param>
/// <param name="time">The clock.</param>
public sealed class ExportCuPdfQueryHandler(IUnitOfWork unitOfWork, ITabularPdfRenderer renderer, ILocalizationService localization, ICurrentTenantService tenant, TimeProvider time)
    : IRequestHandler<ExportCuPdfQuery, GeneratedFile>
{
    /// <inheritdoc/>
    public Task<GeneratedFile> Handle(ExportCuPdfQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Year, cancellationToken);
    }

    private async Task<GeneratedFile> HandleInternalAsync(int year, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<WithholdingCertificate>();
        var certificates = (await repository.FindAsync(c => c.Year == year, cancellationToken).ConfigureAwait(false)).OrderBy(static c => c.RecipientName, StringComparer.OrdinalIgnoreCase).ToList();
        if (certificates.Count == 0)
        {
            throw new InvalidOperationException(VatComplianceErrors.NothingToPrint);
        }

        var pdf = await renderer.RenderAsync(CuPdfContent.Build(year, tenant.TenantName, certificates, key => localization[key]), firstPage: 1, cancellationToken).ConfigureAwait(false);
        var now = time.GetUtcNow().UtcDateTime;

        foreach (var certificate in certificates)
        {
            certificate.Status = Domain.Enums.WithholdingCertificateStatus.Issued;
            certificate.IssuedAtUtc = now;
            repository.Update(certificate);
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new GeneratedFile(
            string.Create(CultureInfo.InvariantCulture, $"CU_{year}.pdf"),
            pdf,
            string.Format(CultureInfo.CurrentCulture, localization["VatCompliance_Export_CuSummary"], certificates.Count));
    }
}
