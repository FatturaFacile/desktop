// <copyright file="GetTenantsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Tenants.Queries;

/// <summary>
/// Query to retrieve all configured tenants/company profiles in the ERP system.
/// </summary>
public sealed record GetTenantsQuery : IRequest<IReadOnlyList<TenantDto>>;
