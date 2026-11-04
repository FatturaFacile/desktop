// <copyright file="UserDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Identity.Application.DTOs;

/// <summary>
/// Data transfer object describing a user profile.
/// </summary>
/// <param name="Id">The unique identifier of the user.</param>
/// <param name="TenantId">The owning tenant identifier.</param>
/// <param name="Email">The user's decrypted plain email address.</param>
/// <param name="PreferredLanguage">The user's preferred language code.</param>
/// <param name="IsActive">Whether the user profile is active.</param>
public sealed record UserDto(
    Guid Id,
    Guid TenantId,
    string Email,
    string PreferredLanguage,
    bool IsActive);
