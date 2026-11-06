// <copyright file="ExportContactDataQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Privacy;

/// <summary>
/// Exports the personal data of a customer or supplier (right of access, art. 25 nLPD, art. 15 GDPR).
/// </summary>
/// <param name="ContactId">The contact.</param>
public sealed record ExportContactDataQuery(Guid ContactId) : IRequest<GeneratedFile>, IAuthorizedRequest
{
    /// <inheritdoc />
    public Permission RequiredPermission => Permission.CommercialRead;
}
