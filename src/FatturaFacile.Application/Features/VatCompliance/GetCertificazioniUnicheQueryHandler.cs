// <copyright file="GetCertificazioniUnicheQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="GetCertificazioniUnicheQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class GetCertificazioniUnicheQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCertificazioniUnicheQuery, IReadOnlyList<WithholdingCertificate>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<WithholdingCertificate>> Handle(GetCertificazioniUnicheQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Year, cancellationToken);
    }

    private async Task<IReadOnlyList<WithholdingCertificate>> HandleInternalAsync(int year, CancellationToken cancellationToken) =>
        [.. (await unitOfWork.Repository<WithholdingCertificate>().FindAsync(c => c.Year == year, cancellationToken).ConfigureAwait(false)).OrderBy(static c => c.RecipientName, StringComparer.OrdinalIgnoreCase)];
}
