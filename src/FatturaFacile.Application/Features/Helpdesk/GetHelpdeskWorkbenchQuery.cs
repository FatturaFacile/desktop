// <copyright file="GetHelpdeskWorkbenchQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Helpdesk;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Helpdesk;

/// <summary>
/// Loads the ticket workbench: sends the queued actions, pulls the breach-risk list and falls back to the cache offline.
/// </summary>
/// <param name="RiskPercent">The minimum elapsed share of the SLA of the tickets to list.</param>
public sealed record GetHelpdeskWorkbenchQuery(int RiskPercent) : IRequest<HelpdeskWorkbench>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
