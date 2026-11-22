// <copyright file="JournalLineInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// One line of an entry being composed.
/// </summary>
/// <param name="AccountId">The account.</param>
/// <param name="Debit">The debit amount.</param>
/// <param name="Credit">The credit amount.</param>
/// <param name="Description">The optional line description.</param>
/// <param name="CostCenterId">The optional cost center.</param>
public sealed record JournalLineInput(Guid AccountId, decimal Debit, decimal Credit, string? Description = null, Guid? CostCenterId = null);
