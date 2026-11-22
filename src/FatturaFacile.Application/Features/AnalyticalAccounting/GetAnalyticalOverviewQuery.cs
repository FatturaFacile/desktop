// <copyright file="GetAnalyticalOverviewQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.AnalyticalAccounting;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.AnalyticalAccounting;

/// <summary>
/// Reads the cached analytical cost accounting overview. It never calls the server, so it works offline.
/// </summary>
public sealed record GetAnalyticalOverviewQuery : IRequest<AnalyticalOverview>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
