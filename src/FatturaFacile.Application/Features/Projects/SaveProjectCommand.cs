// <copyright file="SaveProjectCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Creates or updates a project offline-first.
/// </summary>
/// <param name="Project">The project.</param>
/// <param name="CommandId">The idempotency key.</param>
public sealed record SaveProjectCommand(Project Project, Guid CommandId) : IRequest<ProjectActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
