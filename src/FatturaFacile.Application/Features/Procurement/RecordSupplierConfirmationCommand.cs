// <copyright file="RecordSupplierConfirmationCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Procurement;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Records a supplier order confirmation offline-first.
/// </summary>
/// <param name="Request">The request.</param>
public sealed record RecordSupplierConfirmationCommand(
    SupplierConfirmationRequest Request) : IRequest<ProcurementActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.InvoicesWrite;
}
