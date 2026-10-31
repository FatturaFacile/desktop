// <copyright file="SelectTenantCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common;
using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Tenants;

/// <summary>
/// Command request to switch the current active application workspace to the specified tenant identifier.
/// </summary>
/// <param name="TenantId">The unique identifier of the tenant workspace to activate.</param>
public record SelectTenantCommand(Guid TenantId) : 
    IRequest<TenantDto>, 
    ICommand<TenantDto>;
