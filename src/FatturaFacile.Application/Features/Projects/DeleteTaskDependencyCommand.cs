// <copyright file="DeleteTaskDependencyCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Deletes a dependency.
/// </summary>
/// <param name="ProjectId">The project identifier.</param>
/// <param name="DependencyId">The dependency identifier.</param>
/// <param name="CommandId">The idempotency key.</param>
public sealed record DeleteTaskDependencyCommand(Guid ProjectId, Guid DependencyId, Guid CommandId) : IRequest<ProjectActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
