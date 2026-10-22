// <copyright file="GetTenantsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;

using MediatR;

namespace FatturaFacile.Application.Features.Administration;

/// <summary>
/// Query to retrieve all active tenants available for selection or administration.
/// </summary>
public record GetTenantsQuery() : IRequest<IReadOnlyList<TenantDto>>;
