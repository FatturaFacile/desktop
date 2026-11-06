// <copyright file="GetPayrollF24Query.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Italian;

/// <summary>
/// Prepares the F24 of the withholdings and contributions of an Italian payroll month (due on the 16th of the following month).
/// </summary>
/// <param name="Year">The year.</param>
/// <param name="Month">The month.</param>
public sealed record GetPayrollF24Query(int Year, int Month) : IRequest<GeneratedFile>, IAuthorizedRequest
{
    /// <inheritdoc />
    public Permission RequiredPermission => Permission.PayrollRead;
}
