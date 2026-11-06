// <copyright file="ReopenPayrollRunCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Reopens a closed payroll run (only the most recent one) to correct it.
/// </summary>
/// <param name="RunId">The payroll run identifier.</param>
public sealed record ReopenPayrollRunCommand(Guid RunId) : IRequest<bool>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.PayrollWrite;
}
