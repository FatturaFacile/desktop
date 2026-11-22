// <copyright file="GetProjectEvmProjectsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.ProjectEvm;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.ProjectEvm;

/// <summary>
/// Reads the cached project picker of the profitability view. It never calls the server.
/// </summary>
public sealed record GetProjectEvmProjectsQuery : IRequest<IReadOnlyList<ProjectEvmProject>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingRead;
}
