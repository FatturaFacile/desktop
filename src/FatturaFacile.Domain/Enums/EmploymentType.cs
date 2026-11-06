// <copyright file="EmploymentType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Contract type for human resources and payroll calculation.
/// </summary>
public enum EmploymentType
{
    /// <summary>
    /// Full-time permanent employment.
    /// </summary>
    FullTime = 0,

    /// <summary>
    /// Part-time employment.
    /// </summary>
    PartTime = 1,

    /// <summary>
    /// Independent contractor or external consultant.
    /// </summary>
    Contractor = 2,

    /// <summary>
    /// Hourly or temporary staff.
    /// </summary>
    Hourly = 3,
}
