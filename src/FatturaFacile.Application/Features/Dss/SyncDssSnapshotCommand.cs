// <copyright file="SyncDssSnapshotCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Dss;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Dss;

/// <summary>
/// Command that pulls the Executive DSS snapshot of a filter from the web-api into the local cache.
/// </summary>
/// <param name="Filter">The filter.</param>
public sealed record SyncDssSnapshotCommand(DssFilter Filter) : IRequest<DssSyncResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
