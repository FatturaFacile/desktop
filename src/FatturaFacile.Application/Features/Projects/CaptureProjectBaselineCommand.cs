// <copyright file="CaptureProjectBaselineCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Freezes the current plan of a project as a new baseline.
/// </summary>
/// <param name="ProjectId">The project identifier.</param>
/// <param name="Name">The baseline name.</param>
/// <param name="CommandId">The idempotency key.</param>
public sealed record CaptureProjectBaselineCommand(Guid ProjectId, string Name, Guid CommandId) : IRequest<ProjectActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
