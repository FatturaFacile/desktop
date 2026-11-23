// <copyright file="GetWithholdingPaymentsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Reads the Modello F24 payments of the withholdings of a year.
/// </summary>
/// <param name="Year">The year.</param>
public sealed record GetWithholdingPaymentsQuery(int Year) : IRequest<IReadOnlyList<WithholdingF24Payment>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
