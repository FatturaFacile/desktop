// <copyright file="SaveWbsNodeCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Creates or updates a WBS node offline-first.
/// </summary>
/// <param name="Node">The node.</param>
/// <param name="CommandId">The idempotency key.</param>
public sealed record SaveWbsNodeCommand(WbsNode Node, Guid CommandId) : IRequest<ProjectActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
