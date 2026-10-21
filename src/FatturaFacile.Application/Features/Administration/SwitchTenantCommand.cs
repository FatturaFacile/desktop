// <copyright file="SwitchTenantCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Administration;

/// <summary>
/// Represents switch tenant command.
/// </summary>
/// <param name="TargetTenantId">The target tenant id.</param>
public record SwitchTenantCommand(Guid TargetTenantId) : IRequest<TenantDto>;
