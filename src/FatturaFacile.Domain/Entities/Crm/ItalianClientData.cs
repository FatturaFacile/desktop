// <copyright file="ItalianClientData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Crm;

/// <summary>
/// Data transfer object holding fiscal and contact details required to create an Italian client.
/// </summary>
/// <param name="TenantId">The unique identifier of the owning tenant.</param>
/// <param name="BusinessName">The legal company name or individual full name.</param>
/// <param name="FiscalCode">The Italian tax code (Codice Fiscale).</param>
/// <param name="VatNumber">The Italian VAT number (Partita IVA).</param>
/// <param name="SdiCode">The 7-character SDI recipient code; if omitted or blank, defaults to "0000000".</param>
/// <param name="PecEmail">The certified electronic mail (PEC) address for invoice delivery.</param>
/// <param name="Address">The registered billing and postal address.</param>
/// <param name="Email">The general commercial contact email.</param>
/// <param name="Phone">The optional contact telephone number.</param>
/// <param name="PaymentTermDays">The agreed payment terms in days (defaults to 30 days).</param>
public sealed record ItalianClientData(
    Guid       TenantId,
    string     BusinessName,
    FiscalCode FiscalCode,
    VatNumber  VatNumber,
    string     SdiCode,
    string     PecEmail,
    Address    Address,
    string     Email,
    string?    Phone           = null,
    int        PaymentTermDays = 30);
