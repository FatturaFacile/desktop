// <copyright file="SwissClientData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Crm;

/// <summary>
/// Data transfer object holding fiscal and contact details required to create a Swiss client.
/// </summary>
/// <param name="TenantId">The unique identifier of the owning tenant.</param>
/// <param name="BusinessName">The legal registered company name.</param>
/// <param name="SwissUid">The Swiss enterprise identifier (UID CHE-xxx.xxx.xxx).</param>
/// <param name="CantonalCode">The two-letter Swiss canton abbreviation (e.g., TI, GR, ZH).</param>
/// <param name="QrIban">The optional Swiss QR-IBAN for invoice collections.</param>
/// <param name="Address">The Swiss billing and postal address.</param>
/// <param name="Email">The primary contact email address.</param>
/// <param name="Phone">The optional contact telephone number.</param>
/// <param name="PaymentTermDays">The agreed payment terms in days (defaults to 30 days).</param>
public sealed record SwissClientData(
    Guid    TenantId,
    string  BusinessName,
    string  SwissUid,
    string  CantonalCode,
    QrIban? QrIban,
    Address Address,
    string  Email,
    string? Phone           = null,
    int     PaymentTermDays = 30);
