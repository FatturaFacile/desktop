// <copyright file="ItalianPayrollResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Payroll.Italian;

/// <summary>
/// Outcome of the Italian payroll of one employee.
/// </summary>
/// <param name="Payslip">The payslip.</param>
/// <param name="Lines">The payslip lines.</param>
public sealed record ItalianPayrollResult(Payslip Payslip, IReadOnlyList<PayslipLine> Lines);
