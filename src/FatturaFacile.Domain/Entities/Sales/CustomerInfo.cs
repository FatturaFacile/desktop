// <copyright file="CustomerInfo.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Sales;

/// <summary>
/// Represents the customer information for an invoice, including name, VAT number, fiscal code, recipient code, PEC, and address.
/// </summary>
/// <param name="Name">The legal name of the customer.</param>
/// <param name="VatNumber">The VAT number of the customer.</param>
/// <param name="FiscalCode">The fiscal code of the customer.</param>
/// <param name="RecipientCode">The recipient code for electronic invoicing.</param>
/// <param name="Pec">The PEC (certified email) of the customer.</param>
/// <param name="Address">The address of the customer.</param>
public sealed record CustomerInfo(
    string Name,
    string? VatNumber,
    string? FiscalCode,
    string RecipientCode,
    string? Pec,
    Address Address);
