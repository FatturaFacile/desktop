// <copyright file="CustomerLookupDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Lightweight customer data transfer record optimized for lookup, search, and dropdown binding.
/// </summary>
/// <param name="Id">The unique customer identifier.</param>
/// <param name="DisplayName">The primary display name of the customer entity.</param>
/// <param name="VatOrUid">The VAT number or Swiss UID string for fast identification.</param>
/// <param name="Email">The contact email address.</param>
/// <param name="Country">The two-letter ISO country code.</param>
public record CustomerLookupDto(
    Guid Id,
    string DisplayName,
    string? VatOrUid,
    string? Email,
    string Country);
