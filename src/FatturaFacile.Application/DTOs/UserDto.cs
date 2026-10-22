// <copyright file="UserDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of an application operator or system user account.
/// </summary>
/// <param name="Id">The unique user account identifier.</param>
/// <param name="TenantId">The tenant organization boundary associated with the user.</param>
/// <param name="Username">The unique login username or credential handle.</param>
/// <param name="Email">The corporate electronic mail address for account notifications.</param>
/// <param name="FullName">The full descriptive person or contact name.</param>
/// <param name="Role">The security authorization role granted to the account.</param>
/// <param name="IsActive">A flag indicating whether the user is authorized to sign into the system.</param>
/// <param name="LastLoginUtc">The UTC timestamp when the user last authenticated successfully, or <see langword="null"/> if never logged in.</param>
public record UserDto(
    Guid      Id,
    Guid      TenantId,
    string    Username,
    string    Email,
    string    FullName,
    UserRole  Role,
    bool      IsActive,
    DateTime? LastLoginUtc);
