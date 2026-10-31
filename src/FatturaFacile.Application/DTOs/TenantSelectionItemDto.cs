// <copyright file="TenantSelectionItemDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Lightweight data transfer representation of an organization tenant for fast selection and visual display in the UI.
/// </summary>
/// <param name="Id">The unique tenant identifier.</param>
/// <param name="Name">The display or business name of the tenant organization.</param>
/// <param name="TaxIdDisplay">The formatted tax identifier (VAT number, Swiss UID, or fiscal code) for user display.</param>
/// <param name="DefaultCurrency">The base default currency configured for the tenant (e.g., EUR, CHF).</param>
/// <param name="Initials">The two-letter initials extracted from the tenant name for badge visualization.</param>
/// <param name="Country">The two-letter ISO country code governing the tenant's primary fiscal jurisdiction.</param>
public record TenantSelectionItemDto(
    Guid Id,
    string Name,
    string TaxIdDisplay,
    string DefaultCurrency,
    string Initials,
    string Country);
