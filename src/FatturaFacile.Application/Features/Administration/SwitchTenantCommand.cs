// <copyright file="SwitchTenantCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;

using MediatR;

namespace FatturaFacile.Application.Features.Administration;

/// <summary>
/// Command to switch the active operator execution context to a different tenant organization.
/// </summary>
/// <param name="TargetTenantId">The unique identifier of the destination tenant organization.</param>
public record SwitchTenantCommand(Guid TargetTenantId) : IRequest<TenantDto>;
