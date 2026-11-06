// <copyright file="ICountryPayrollStrategy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Strategies;

/// <summary>
/// Strategy contract for computing country-specific payroll deductions and employer social contributions.
/// </summary>
public interface ICountryPayrollStrategy
{
    /// <summary>
    /// Gets the ISO country code this strategy handles ("CH" or "IT").
    /// </summary>
    public string CountryCode { get; }

    /// <summary>
    /// Computes monthly compensation and statutory deductions for an employee.
    /// </summary>
    /// <param name="employee">The employee profile.</param>
    /// <param name="targetMonth">The payroll calendar month.</param>
    /// <returns>A detailed breakdown of gross, deductions, and net salary.</returns>
    public PayrollCalculationResultDto CalculatePayroll(HrEmployee employee, DateTime targetMonth);
}
