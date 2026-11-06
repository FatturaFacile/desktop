// <copyright file="GetUniEmensDataQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Italian;

/// <summary>
/// Prepares the monthly data of the UniEmens individual declarations (to be loaded in the INPS sending software).
/// </summary>
/// <param name="Year">The year.</param>
/// <param name="Month">The month.</param>
public sealed record GetUniEmensDataQuery(int Year, int Month) : IRequest<GeneratedFile>, IAuthorizedRequest
{
    /// <inheritdoc />
    public Permission RequiredPermission => Permission.PayrollRead;
}
