// <copyright file="IssueDropshipDdtCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Procurement;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Issues the custom transport document of a dropshipment offline-first.
/// </summary>
/// <param name="Request">The request.</param>
public sealed record IssueDropshipDdtCommand(
    IssueDdtRequest Request) : IRequest<ProcurementActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.InvoicesWrite;
}
