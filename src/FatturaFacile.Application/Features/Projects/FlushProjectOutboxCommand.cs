// <copyright file="FlushProjectOutboxCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Replays the project changes made offline.
/// </summary>
public sealed record FlushProjectOutboxCommand : IRequest<ProjectFlushResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
