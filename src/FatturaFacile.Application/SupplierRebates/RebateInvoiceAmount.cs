// <copyright file="RebateInvoiceAmount.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// A supplier document that qualifies for a rebate agreement.
/// </summary>
/// <param name="DocumentType">The document type (TD04 is a credit note).</param>
/// <param name="NetAmount">The net amount as booked on the document.</param>
public sealed record RebateInvoiceAmount(string DocumentType, decimal NetAmount);
