// <copyright file="SupplierDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of an external vendor or commercial supplier profile.
/// </summary>
/// <param name="Id">The unique supplier identifier.</param>
/// <param name="TenantId">The tenant organization boundary owning this supplier entry.</param>
/// <param name="Name">The registered commercial or corporate name of the vendor.</param>
/// <param name="Jurisdiction">The primary operational and fiscal jurisdiction governing the supplier.</param>
/// <param name="VatNumber">The value-added tax identification number, if registered.</param>
/// <param name="SwissUid">The Swiss enterprise identification number (UID), if applicable.</param>
/// <param name="SdiCode">The SDI recipient code for Italian electronic B2B communication.</param>
/// <param name="AddressFormatted">The fully formatted billing and postal address string.</param>
/// <param name="Iban">The primary International Bank Account Number for outbound vendor disbursements.</param>
/// <param name="PaymentTermsDays">The standard agreed settlement terms, expressed in days from bill receipt.</param>
/// <param name="IsActive">A flag indicating whether procurement transactions with this vendor are currently permitted.</param>
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
