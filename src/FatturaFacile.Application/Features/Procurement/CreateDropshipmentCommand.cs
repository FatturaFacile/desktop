// <copyright file="CreateDropshipmentCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Procurement;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Creates the supplier order of a dropship sales order offline-first.
/// </summary>
/// <param name="Request">The request.</param>
public sealed record CreateDropshipmentCommand(
    CreateDropshipmentRequest Request) : IRequest<ProcurementActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.InvoicesWrite;
}
