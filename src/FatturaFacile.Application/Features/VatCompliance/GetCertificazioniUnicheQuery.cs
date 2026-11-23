// <copyright file="GetCertificazioniUnicheQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Reads the Certificazioni Uniche of a year.
/// </summary>
/// <param name="Year">The year of the payments.</param>
public sealed record GetCertificazioniUnicheQuery(int Year) : IRequest<IReadOnlyList<WithholdingCertificate>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
