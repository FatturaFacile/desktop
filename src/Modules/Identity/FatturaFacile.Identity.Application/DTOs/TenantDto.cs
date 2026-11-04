// <copyright file="TenantDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Identity.Application.DTOs;

/// <summary>
/// Data transfer object describing a tenant account.
/// </summary>
/// <param name="Id">The unique identifier of the tenant.</param>
/// <param name="Name">The display name of the tenant organization.</param>
/// <param name="Slug">The unique URL slug of the tenant.</param>
/// <param name="IsActive">Whether the tenant account is currently active.</param>
public sealed record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive);
