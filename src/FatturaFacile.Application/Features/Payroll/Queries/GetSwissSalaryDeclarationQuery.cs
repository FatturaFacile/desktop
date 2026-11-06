// <copyright file="GetSwissSalaryDeclarationQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Queries;

/// <summary>
/// Prepares the yearly salary declaration for AHV/AVS, ALV, UVG/LAINF, KTG and BVG (the data sent with Swissdec ELM),
/// from the closed payroll runs.
/// </summary>
/// <param name="Year">The year.</param>
public sealed record GetSwissSalaryDeclarationQuery(int Year) : IRequest<GeneratedFile>, IAuthorizedRequest
{
    /// <inheritdoc />
    public Permission RequiredPermission => Permission.PayrollRead;
}
