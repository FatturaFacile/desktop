// <copyright file="GrantCreditOverrideCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Grants a credit override.
/// </summary>
/// <param name="Request">The override.</param>
public sealed record GrantCreditOverrideCommand(GrantOverrideRequest Request) : IRequest<ReceivablesActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
