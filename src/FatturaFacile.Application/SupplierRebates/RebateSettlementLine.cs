// <copyright file="RebateSettlementLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// A row of the analytical breakdown of a settlement proposal.
/// </summary>
/// <param name="LineType">The row type: invoice, credit_note, contribution or bonus.</param>
/// <param name="Reference">The document reference.</param>
/// <param name="ReferenceDate">The document date.</param>
/// <param name="NetAmount">The net amount of the document.</param>
/// <param name="RebateAmount">The share of the rebate that comes from the row.</param>
public sealed record RebateSettlementLine(string LineType, string Reference, DateOnly? ReferenceDate, decimal NetAmount, decimal RebateAmount);
