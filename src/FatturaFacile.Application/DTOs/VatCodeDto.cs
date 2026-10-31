// <copyright file="VatCodeDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of a value-added tax rate code and fiscal treatment classification.
/// </summary>
/// <param name="Id">The unique tax rate identifier.</param>
/// <param name="Code">The fiscal system short identification code (e.g., &quot;22%&quot;, &quot;8.1%&quot;, &quot;EXEMPT&quot;).</param>
/// <param name="Description">The official description or legal justification of the tax treatment.</param>
/// <param name="Rate">The numerical tax percentage rate applied to taxable amounts.</param>
/// <param name="CountryIso">The two-letter ISO country code governing the tax code.</param>
/// <param name="IsReverseCharge">A value indicating whether reverse charge accounting rules apply.</param>
/// <param name="IsSplitPayment">A value indicating whether split payment mechanisms for public administration apply.</param>
public record VatCodeDto(
    Guid Id,
    string Code,
    string Description,
    decimal Rate,
    string CountryIso,
    bool IsReverseCharge,
    bool IsSplitPayment);
