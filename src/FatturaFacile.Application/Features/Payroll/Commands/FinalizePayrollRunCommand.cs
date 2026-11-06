// <copyright file="FinalizePayrollRunCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Closes a payroll run: its payslips can no longer be recalculated and are used for the salary certificates.
/// </summary>
/// <param name="RunId">The payroll run identifier.</param>
public sealed record FinalizePayrollRunCommand(Guid RunId) : IRequest<bool>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.PayrollWrite;
}
