// <copyright file="SupplierInfo.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Sales;

/// <summary>
/// Represents the supplier information for an invoice, including name, VAT number, fiscal code, and address.
/// </summary>
/// <param name="Name">The legal name of the supplier.</param>
/// <param name="VatNumber">The VAT number of the supplier.</param>
/// <param name="FiscalCode">The fiscal code of the supplier.</param>
/// <param name="Address">The address of the supplier.</param>
public sealed record SupplierInfo(
    string Name,
    string VatNumber,
    string FiscalCode,
    Address Address);
