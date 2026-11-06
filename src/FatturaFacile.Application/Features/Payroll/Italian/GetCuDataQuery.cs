// <copyright file="GetCuDataQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Italian;

/// <summary>
/// Prepares the yearly data of the Certificazione Unica of the employees.
/// </summary>
/// <param name="Year">The year.</param>
public sealed record GetCuDataQuery(int Year) : IRequest<GeneratedFile>, IAuthorizedRequest
{
    /// <inheritdoc />
    public Permission RequiredPermission => Permission.PayrollRead;
}
