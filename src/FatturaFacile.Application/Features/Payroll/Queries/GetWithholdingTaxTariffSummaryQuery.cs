// <copyright file="GetWithholdingTaxTariffSummaryQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Queries;

/// <summary>
/// Returns the imported withholding tax tariffs by canton and validity date.
/// </summary>
public sealed record GetWithholdingTaxTariffSummaryQuery : IRequest<IReadOnlyList<WithholdingTaxTariffSummaryDto>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.PayrollRead;
}
