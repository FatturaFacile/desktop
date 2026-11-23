// <copyright file="PullProjectPortfolioCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Refreshes the local portfolio from the web-api.
/// </summary>
public sealed record PullProjectPortfolioCommand : IRequest<ProjectPullResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
