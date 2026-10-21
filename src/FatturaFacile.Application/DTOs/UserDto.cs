// <copyright file="UserDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Represents user dto.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="TenantId">The tenant id.</param>
/// <param name="Username">The username.</param>
/// <param name="Email">The email.</param>
/// <param name="FullName">The full name.</param>
/// <param name="Role">The role.</param>
/// <param name="IsActive">The is active.</param>
/// <param name="LastLoginUtc">The last login utc.</param>
public record UserDto(
    Guid Id,
    Guid TenantId,
    string Username,
    string Email,
    string FullName,
    UserRole Role,
    bool IsActive,
    DateTime? LastLoginUtc);
