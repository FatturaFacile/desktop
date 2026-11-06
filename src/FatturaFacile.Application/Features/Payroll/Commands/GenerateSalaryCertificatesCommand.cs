// <copyright file="GenerateSalaryCertificatesCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Computes the salary certificates (form 11) of a year from the closed payroll runs.
/// </summary>
/// <param name="Year">The year.</param>
public sealed record GenerateSalaryCertificatesCommand(int Year) : IRequest<int>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.PayrollWrite;
}
