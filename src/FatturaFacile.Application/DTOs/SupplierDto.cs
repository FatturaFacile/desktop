// <copyright file="SupplierDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Represents supplier dto.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="TenantId">The tenant id.</param>
/// <param name="Name">The name.</param>
/// <param name="Jurisdiction">The jurisdiction.</param>
/// <param name="VatNumber">The vat number.</param>
/// <param name="SwissUid">The swiss uid.</param>
/// <param name="SdiCode">The sdi code.</param>
/// <param name="AddressFormatted">The address formatted.</param>
/// <param name="Iban">The iban.</param>
/// <param name="PaymentTermsDays">The payment terms days.</param>
/// <param name="IsActive">The is active.</param>
public record SupplierDto(
    Guid Id,
    Guid TenantId,
    string Name,
    Jurisdiction Jurisdiction,
    string? VatNumber,
    string? SwissUid,
    string? SdiCode,
    string AddressFormatted,
    string? Iban,
    int PaymentTermsDays,
    bool IsActive);
