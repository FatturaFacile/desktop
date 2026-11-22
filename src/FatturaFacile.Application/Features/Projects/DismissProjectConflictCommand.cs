// <copyright file="DismissProjectConflictCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Discards a project change the web-api refused.
/// </summary>
/// <param name="EntryId">The outbox entry identifier.</param>
public sealed record DismissProjectConflictCommand(Guid EntryId) : IRequest, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
