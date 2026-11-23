// <copyright file="CalculateSwissVatReturnCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.SwissVat;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.SwissVat;

/// <summary>
/// CQRS command to calculate a Swiss VAT period return and its quadratura report.
/// </summary>
/// <param name="Input">Calculation input lines and settings.</param>
/// <param name="UidMwst">Configured company UID-MWST identifier.</param>
/// <param name="PendingBezugsteuerCount">Number of unreviewed detected Bezugsteuer entries.</param>
public sealed record CalculateSwissVatReturnCommand(
    SwissVatCalculationInput Input,
    string? UidMwst = null,
    int PendingBezugsteuerCount = 0)
    : IRequest<CalculateSwissVatReturnOutcome>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
