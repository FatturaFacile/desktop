// <copyright file="RegisterTenantUserCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Shared.Contracts;
using FatturaFacile.Identity.Application.DTOs;

namespace FatturaFacile.Identity.Application.Commands;

/// <summary>
/// Command to register a new tenant organisation and provision its initial administrator user.
/// </summary>
/// <param name="TenantName">The display name of the new tenant company.</param>
/// <param name="TenantSlug">The desired URL slug and identifier for the tenant.</param>
/// <param name="Email">The administrator user's email address.</param>
/// <param name="Password">The administrator user's password.</param>
/// <param name="PreferredLanguage">The administrator's preferred culture code (defaults to "en").</param>
public sealed record RegisterTenantUserCommand(
    string TenantName,
    string TenantSlug,
    string Email,
    string Password,
    string PreferredLanguage = "en") : ICommand<UserDto>;
