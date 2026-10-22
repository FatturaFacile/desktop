// <copyright file="LoginCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;

using MediatR;

namespace FatturaFacile.Application.Features.Administration;

/// <summary>
/// Command to authenticate an operator against a designated tenant using login credentials.
/// </summary>
/// <param name="Username">The login username or account handle.</param>
/// <param name="Password">The plain text password to authenticate.</param>
/// <param name="TenantId">The unique identifier of the target tenant organization.</param>
public record LoginCommand(
    string Username,
    string Password,
    Guid   TenantId) : IRequest<UserDto>;
