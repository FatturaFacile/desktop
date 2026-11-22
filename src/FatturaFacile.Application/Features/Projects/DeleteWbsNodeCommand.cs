// <copyright file="DeleteWbsNodeCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Deletes a WBS node with its descendants.
/// </summary>
/// <param name="ProjectId">The project identifier.</param>
/// <param name="NodeId">The node identifier.</param>
/// <param name="CommandId">The idempotency key.</param>
public sealed record DeleteWbsNodeCommand(Guid ProjectId, Guid NodeId, Guid CommandId) : IRequest<ProjectActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
