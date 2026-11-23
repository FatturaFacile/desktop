// <copyright file="BuildCertificazioniUnicheCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="BuildCertificazioniUnicheCommand"/>: rebuilds the draft certificates of a year; certificates already issued
/// are never rewritten.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class BuildCertificazioniUnicheCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<BuildCertificazioniUnicheCommand, int>
{
    /// <inheritdoc/>
    public Task<int> Handle(BuildCertificazioniUnicheCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Year, cancellationToken);
    }

    private async Task<int> HandleInternalAsync(int year, CancellationToken cancellationToken)
    {
        var lines = await unitOfWork.Repository<WithholdingLine>().FindAsync(l => l.Year == year, cancellationToken).ConfigureAwait(false);
        var payments = await unitOfWork.Repository<WithholdingF24Payment>().FindAsync(p => p.Year == year, cancellationToken).ConfigureAwait(false);
        var contacts = await IntentLetterLookup.ContactsAsync(unitOfWork, lines.Select(static l => l.SupplierId), cancellationToken).ConfigureAwait(false);
        var repository = unitOfWork.Repository<WithholdingCertificate>();
        var existing = await repository.FindAsync(c => c.Year == year, cancellationToken).ConfigureAwait(false);

        var built = CertificazioneUnicaBuilder.Build(year, lines, contacts, payments);
        var count = 0;

        foreach (var certificate in built)
        {
            var current = existing.FirstOrDefault(c => c.SupplierId == certificate.SupplierId && string.Equals(c.Reason, certificate.Reason, StringComparison.Ordinal));
            if (current is null)
            {
                await repository.AddAsync(certificate, cancellationToken).ConfigureAwait(false);
                count++;
            }
            else if (current.Status == WithholdingCertificateStatus.Draft)
            {
                current.RecipientName = certificate.RecipientName;
                current.RecipientTaxCode = certificate.RecipientTaxCode;
                current.RecipientVatNumber = certificate.RecipientVatNumber;
                current.GrossPaid = certificate.GrossPaid;
                current.TaxableAmount = certificate.TaxableAmount;
                current.WithheldAmount = certificate.WithheldAmount;
                current.WithheldPaidAmount = certificate.WithheldPaidAmount;
                repository.Update(current);
                count++;
            }
        }

        // A draft whose withholdings disappeared (a settlement was recalculated) is dropped.
        foreach (var stale in existing.Where(c => c.Status == WithholdingCertificateStatus.Draft && !built.Any(b => b.SupplierId == c.SupplierId && string.Equals(b.Reason, c.Reason, StringComparison.Ordinal))))
        {
            repository.Delete(stale);
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return count;
    }
}
