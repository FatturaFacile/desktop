// <copyright file="GetProjectEvmSnapshotQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ProjectEvm;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ProjectEvm;

/// <summary>
/// Reads the cached profitability snapshot of a project. It never calls the server, so it works offline.
/// </summary>
/// <param name="ProjectId">The project identifier.</param>
public sealed record GetProjectEvmSnapshotQuery(string ProjectId) : IRequest<ProjectEvmSnapshot?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
