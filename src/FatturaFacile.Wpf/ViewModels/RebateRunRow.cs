// <copyright file="RebateRunRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One accrual run of an agreement, formatted for display.
/// </summary>
/// <param name="Id">The run identifier.</param>
/// <param name="Period">The period of the run.</param>
/// <param name="Status">The localized status.</param>
/// <param name="BaseAmount">The qualifying spend.</param>
/// <param name="Cumulative">The cumulative rebate.</param>
/// <param name="Delta">The accrual of the period.</param>
/// <param name="Run">The run behind the row.</param>
internal sealed record RebateRunRow(string Id, string Period, string Status, string BaseAmount, string Cumulative, string Delta, RebateAccrualRun Run);
