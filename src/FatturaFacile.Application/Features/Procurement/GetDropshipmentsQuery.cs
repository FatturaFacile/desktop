// <copyright file="GetDropshipmentsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Procurement;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Reads the dropshipments, optionally of one status.
/// </summary>
/// <param name="Status">The wire status, or null for all.</param>
public sealed record GetDropshipmentsQuery(
    string? Status) : IRequest<ProcurementLoad<IReadOnlyList<DropshipmentView>>?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
