// <copyright file="InstantiateProjectTemplateCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Creates a project from a template or an existing project (#186).
/// </summary>
/// <param name="Request">The choices of the user.</param>
/// <param name="CommandId">The idempotency key.</param>
public sealed record InstantiateProjectTemplateCommand(TemplateInstantiationRequest Request, Guid CommandId) : IRequest<ProjectActionResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
