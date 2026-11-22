// <copyright file="SaveTaskDependencyCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Creates or updates a dependency; a cycle is rejected.
/// </summary>
/// <param name="Dependency">The dependency.</param>
/// <param name="CommandId">The idempotency key.</param>
public sealed record SaveTaskDependencyCommand(TaskDependency Dependency, Guid CommandId) : IRequest<ProjectActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
