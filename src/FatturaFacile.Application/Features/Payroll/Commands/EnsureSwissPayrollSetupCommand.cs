// <copyright file="EnsureSwissPayrollSetupCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Creates, when missing, the payroll settings and wage type catalog of the current Swiss company and the default family allowance amounts.
/// </summary>
public sealed record EnsureSwissPayrollSetupCommand : IRequest<bool>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.PayrollWrite;
}
