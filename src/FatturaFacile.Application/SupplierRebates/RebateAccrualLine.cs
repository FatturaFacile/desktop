// <copyright file="RebateAccrualLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// A supplier invoice or credit note that contributed to an accrual run.
/// </summary>
/// <param name="PassiveInvoiceId">The passive invoice identifier.</param>
/// <param name="InvoiceNumber">The invoice number.</param>
/// <param name="DocumentType">The document type (TD04 is a credit note).</param>
/// <param name="InvoiceDate">The invoice date.</param>
/// <param name="NetAmount">The signed net amount (negative for a credit note).</param>
public sealed record RebateAccrualLine(string PassiveInvoiceId, string InvoiceNumber, string DocumentType, DateOnly? InvoiceDate, decimal NetAmount);
