// <copyright file="SwissPayrollResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Payroll.Swiss;

/// <summary>
/// The calculated payslip and its lines (identifiers and run are assigned by the caller).
/// </summary>
/// <param name="Payslip">The payslip.</param>
/// <param name="Lines">The lines.</param>
public sealed record SwissPayrollResult(Payslip Payslip, IReadOnlyList<PayslipLine> Lines);
