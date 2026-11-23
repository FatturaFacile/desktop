// <copyright file="SaveCreditLimitCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Receivables;

/// <summary>
/// Creates or updates a credit limit.
/// </summary>
/// <param name="Request">The limit.</param>
public sealed record SaveCreditLimitCommand(CreditLimitRequest Request) : IRequest<ReceivablesActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
