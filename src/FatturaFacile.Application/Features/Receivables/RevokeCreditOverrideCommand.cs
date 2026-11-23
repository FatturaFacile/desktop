// <copyright file="RevokeCreditOverrideCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Revokes a credit override.
/// </summary>
/// <param name="Request">The revocation.</param>
public sealed record RevokeCreditOverrideCommand(RevokeOverrideRequest Request) : IRequest<ReceivablesActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
