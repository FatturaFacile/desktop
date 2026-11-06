// <copyright file="ExportLipeQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Taxation;

/// <summary>
/// Writes the LIPE file (Comunicazione liquidazioni periodiche IVA) of an Italian VAT return.
/// </summary>
/// <param name="VatReturnId">The VAT return.</param>
public sealed record ExportLipeQuery(Guid VatReturnId) : IRequest<GeneratedFile>, IAuthorizedRequest
{
    /// <inheritdoc />
    public Permission RequiredPermission => Permission.AccountingRead;
}
