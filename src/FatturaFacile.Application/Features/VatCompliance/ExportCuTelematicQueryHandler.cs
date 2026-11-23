// <copyright file="ExportCuTelematicQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="ExportCuTelematicQuery"/>: writes the working file of the certificates and freezes them as issued.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="localization">The localization service.</param>
/// <param name="tenant">The active tenant.</param>
/// <param name="time">The clock.</param>
public sealed class ExportCuTelematicQueryHandler(IUnitOfWork unitOfWork, ILocalizationService localization, ICurrentTenantService tenant, TimeProvider time)
    : IRequestHandler<ExportCuTelematicQuery, GeneratedFile>
{
    /// <inheritdoc/>
    public Task<GeneratedFile> Handle(ExportCuTelematicQuery request, CancellationToken cancellationToken)
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

        var company = await unitOfWork.Repository<Tenant>().GetByIdAsync(tenant.TenantId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException(VatComplianceErrors.ReturnNotFound);
        var taxCode = string.IsNullOrWhiteSpace(company.FiscalCode) ? company.VatNumber ?? string.Empty : company.FiscalCode;
        var now = time.GetUtcNow().UtcDateTime;

        foreach (var certificate in certificates)
        {
            certificate.Status = WithholdingCertificateStatus.Issued;
            certificate.IssuedAtUtc = now;
            repository.Update(certificate);
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new GeneratedFile(
            CuTelematicFileBuilder.FileName(taxCode.Trim().ToUpperInvariant(), year),
            CuTelematicFileBuilder.Build(taxCode.Trim().ToUpperInvariant(), year, certificates),
            string.Format(CultureInfo.CurrentCulture, localization["VatCompliance_Export_CuTelematicSummary"], certificates.Count));
    }
}
