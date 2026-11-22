// <copyright file="GetProjectPortfolioQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Projects;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Projects;

/// <summary>
/// Loads the project portfolio: replays the queue, optionally refreshes from the web-api and reads the local database.
/// </summary>
/// <param name="Refresh">Whether to pull the web-api first.</param>
public sealed record GetProjectPortfolioQuery(bool Refresh) : IRequest<ProjectPortfolio>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
