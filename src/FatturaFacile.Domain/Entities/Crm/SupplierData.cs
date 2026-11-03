// <copyright file="SupplierData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Crm;

/// <summary>
/// Data transfer object holding values required to create a new <see cref="Supplier"/>.
/// </summary>
/// <param name="TenantId">The unique tenant identifier.</param>
/// <param name="Name">The legal company or trade name of the supplier.</param>
/// <param name="Jurisdiction">The fiscal tax jurisdiction governing the supplier (Italy or Switzerland).</param>
/// <param name="Address">The physical or postal address of the vendor.</param>
/// <param name="VatNumber">The optional VAT registration number (Partita IVA or TVA/MWST).</param>
/// <param name="SwissUid">The optional Swiss Business Identification Number (UID CHE-xxx.xxx.xxx).</param>
/// <param name="SdiCode">The optional Italian SDI electronic routing code or PEC address.</param>
/// <param name="Iban">The vendor bank account IBAN for invoice payments.</param>
/// <param name="BicSwift">The SWIFT/BIC code for bank routing.</param>
/// <param name="PaymentTermsDays">The standard agreed payment terms in days (defaults to 30 days).</param>
public sealed record SupplierData(
    Guid         TenantId,
    string       Name,
    Jurisdiction Jurisdiction,
    Address      Address,
    VatNumber?   VatNumber        = null,
    string?      SwissUid         = null,
    string?      SdiCode          = null,
    string?      Iban             = null,
    string?      BicSwift         = null,
    int          PaymentTermsDays = 30);
