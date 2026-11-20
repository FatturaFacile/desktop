// <copyright file="GetQuotesQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Sales.Queries;

/// <summary>
/// Query to retrieve the quotes of the current tenant.
/// </summary>
public sealed record GetQuotesQuery : IRequest<IReadOnlyList<QuoteDto>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
