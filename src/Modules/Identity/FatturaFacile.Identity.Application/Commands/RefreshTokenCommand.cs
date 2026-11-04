// <copyright file="RefreshTokenCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Shared.Contracts;
using FatturaFacile.Identity.Application.DTOs;

namespace FatturaFacile.Identity.Application.Commands;

/// <summary>
/// Command to rotate an expired or expiring access token using a valid refresh token.
/// </summary>
/// <param name="RefreshToken">The plaintext refresh token string.</param>
/// <param name="IpAddress">The optional client IP address for security logging.</param>
public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress = null) : ICommand<AuthResponseDto>;
