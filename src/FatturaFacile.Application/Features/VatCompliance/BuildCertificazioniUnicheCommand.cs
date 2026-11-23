// <copyright file="BuildCertificazioniUnicheCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Builds the Certificazioni Uniche of a year from the withholdings.
/// </summary>
/// <param name="Year">The year of the payments.</param>
public sealed record BuildCertificazioniUnicheCommand(int Year) : IRequest<int>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
