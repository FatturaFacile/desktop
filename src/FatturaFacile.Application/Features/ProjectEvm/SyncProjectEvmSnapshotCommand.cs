// <copyright file="SyncProjectEvmSnapshotCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ProjectEvm;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ProjectEvm;

/// <summary>
/// Pulls the profitability snapshot of a project from the server into the local cache. With a null project it pulls the project
/// picker instead.
/// </summary>
/// <param name="ProjectId">The project identifier, or null for the project picker.</param>
public sealed record SyncProjectEvmSnapshotCommand(string? ProjectId) : IRequest<ProjectEvmSyncResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
