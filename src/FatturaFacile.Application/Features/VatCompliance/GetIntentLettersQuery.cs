// <copyright file="GetIntentLettersQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Reads the declarations of intent with their erosion and residual plafond.
/// </summary>
/// <param name="Year">The year, or null for all.</param>
public sealed record GetIntentLettersQuery(int? Year) : IRequest<IReadOnlyList<IntentLetterView>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
