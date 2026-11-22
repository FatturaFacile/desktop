// <copyright file="ProjectEvmWaterfallStep.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectEvm;

/// <summary>
/// One step of the profitability waterfall: the recognised revenue, a cost source or the gross margin.
/// </summary>
/// <param name="Key">The step key (revenue, TimesheetLabor, EmployeeExpense, VendorInvoice, WarehouseMaterialIssue, OverheadAllocation, Adjustment, gross_margin).</param>
/// <param name="Amount">The signed amount of the step.</param>
/// <param name="RunningTotal">The running total after the step.</param>
/// <param name="IsTotal">True for the revenue and the gross margin bars, false for a cost step.</param>
public sealed record ProjectEvmWaterfallStep(string Key, double Amount, double RunningTotal, bool IsTotal);
