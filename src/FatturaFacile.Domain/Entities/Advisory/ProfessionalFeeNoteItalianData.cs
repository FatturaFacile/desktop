// <copyright file="ProfessionalFeeNoteItalianData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Entities.Advisory;

/// <summary>
/// Data transfer object holding tax, contribution, and matter values required to generate an Italian professional fee note.
/// </summary>
/// <param name="TenantId">The unique tenant identifier.</param>
/// <param name="DocNumber">The unique reference document number.</param>
/// <param name="MatterId">The associated client matter identifier.</param>
/// <param name="Date">The issuance date of the fee note.</param>
/// <param name="GrossFees">The gross taxable professional fees.</param>
/// <param name="NonTaxableExpenses">Out-of-pocket expenses exempt from VAT under Italian DPR 633/72 art. 15.</param>
/// <param name="VatRate">The standard or applicable VAT rate percentage (defaults to 22.0%).</param>
/// <param name="CassaPercent">The mandatory pension fund contribution percentage (defaults to 4.0%).</param>
/// <param name="RitenutaPercent">The statutory withholding tax percentage (defaults to 20.0%).</param>
public sealed record ProfessionalFeeNoteItalianData(
    Guid     TenantId,
    string   DocNumber,
    Guid     MatterId,
    DateTime Date,
    decimal  GrossFees,
    decimal  NonTaxableExpenses,
    decimal  VatRate         = 22.0m,
    decimal  CassaPercent    = 4.0m,
    decimal  RitenutaPercent = 20.0m);
