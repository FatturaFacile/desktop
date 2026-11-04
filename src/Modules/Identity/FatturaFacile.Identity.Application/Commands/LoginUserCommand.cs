// <copyright file="LoginUserCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Shared.Contracts;
using FatturaFacile.Identity.Application.DTOs;

namespace FatturaFacile.Identity.Application.Commands;

/// <summary>
/// Command to authenticate an existing tenant user and generate JWT tokens.
/// </summary>
/// <param name="TenantSlug">The target workspace/tenant unique slug.</param>
/// <param name="Email">The user's registered email address.</param>
/// <param name="Password">The user's plaintext password.</param>
/// <param name="IpAddress">The optional client IP address for security logging.</param>
/// <param name="UserAgent">The optional client User-Agent string.</param>
public sealed record LoginUserCommand(
    string TenantSlug,
    string Email,
    string Password,
    string? IpAddress = null,
    string? UserAgent = null) : ICommand<AuthResponseDto>;
