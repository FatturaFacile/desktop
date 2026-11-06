// <copyright file="CalculatePayrollRunCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Calculates (or recalculates) the payroll run of a month for all employees employed in the month.
/// </summary>
/// <param name="Year">The year.</param>
/// <param name="Month">The month.</param>
/// <param name="PaymentDate">The payment date.</param>
public sealed record CalculatePayrollRunCommand(int Year, int Month, DateTime? PaymentDate) : IRequest<Guid>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.PayrollWrite;
}
