// <copyright file="SwitchTenantCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;

namespace FatturaFacile.Application.Features.Tenants.Commands;

/// <summary>
/// Command to switch the active tenant context for the ERP application session.
/// </summary>
/// <param name="TenantId">The unique identifier of the target tenant.</param>
public sealed record SwitchTenantCommand(Guid TenantId) : IRequest<bool>;
