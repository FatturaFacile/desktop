// <copyright file="GetTenantsForUserQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common;
using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Tenants;

/// <summary>
/// Query request to retrieve all tenant organizations and workspaces associated with a specific Windows user.
/// </summary>
/// <param name="WindowsUsername">The Windows login identity or domain username.</param>
public record GetTenantsForUserQuery(string WindowsUsername) : 
    IRequest<IReadOnlyList<TenantDto>>, 
    IQuery<IReadOnlyList<TenantDto>>;
