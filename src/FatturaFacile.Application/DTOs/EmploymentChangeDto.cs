// <copyright file="EmploymentChangeDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// An entry or exit to notify (AHV compensation office, withholding tax authority, accident and pension insurers).
/// </summary>
/// <param name="EmployeeName">The employee name.</param>
/// <param name="Change">"Entrata" or "Uscita".</param>
/// <param name="Date">The entry or exit date.</param>
/// <param name="AhvNumber">The AHV number.</param>
/// <param name="ResidencePermit">The residence permit.</param>
/// <param name="WithholdingTax">Whether the employee is subject to withholding tax (notification within 8 days).</param>
/// <param name="Recipients">The offices to notify.</param>
public sealed record EmploymentChangeDto(
    string EmployeeName,
    string Change,
    DateTime Date,
    string? AhvNumber,
    string? ResidencePermit,
    bool WithholdingTax,
    string Recipients);
