// <copyright file="GetTenantsQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Administration;

/// <summary>
/// Represents get tenants query.
/// </summary>
public record GetTenantsQuery() : IRequest<IReadOnlyList<TenantDto>>;
