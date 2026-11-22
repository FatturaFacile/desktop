// <copyright file="TemplateInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// The values the user gives to a template.
/// </summary>
/// <param name="Amount">The net amount.</param>
/// <param name="TaxRate">The tax rate in percent, when the template needs one.</param>
/// <param name="Accounts">The account (code or identifier) chosen for each account parameter key.</param>
/// <param name="CostCenterId">The optional cost center put on every line.</param>
public sealed record TemplateInput(decimal Amount, decimal? TaxRate, IReadOnlyDictionary<string, string> Accounts, Guid? CostCenterId = null);
