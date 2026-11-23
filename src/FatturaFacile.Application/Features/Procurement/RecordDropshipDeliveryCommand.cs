// <copyright file="RecordDropshipDeliveryCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Procurement;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Records the delivery notice of a dropshipment offline-first.
/// </summary>
/// <param name="Request">The request.</param>
public sealed record RecordDropshipDeliveryCommand(
    RecordDeliveryRequest Request) : IRequest<ProcurementActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.InvoicesWrite;
}
