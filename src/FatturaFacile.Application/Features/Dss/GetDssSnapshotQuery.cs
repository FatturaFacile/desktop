// <copyright file="GetDssSnapshotQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Dss;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Dss;

/// <summary>
/// Query for the Executive DSS snapshot cached for a filter. It reads the local cache only, so it works offline.
/// </summary>
/// <param name="Filter">The filter.</param>
public sealed record GetDssSnapshotQuery(DssFilter Filter) : IRequest<DssSnapshot?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
