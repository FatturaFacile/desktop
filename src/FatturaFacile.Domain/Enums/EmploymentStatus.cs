// <copyright file="EmploymentStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Status of an employee within the organization.
/// </summary>
public enum EmploymentStatus
{
    /// <summary>
    /// Employee is undergoing onboarding.
    /// </summary>
    Onboarding = 0,

    /// <summary>
    /// Employee is active.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Employee is suspended or on extended leave.
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// Employment contract has been terminated.
    /// </summary>
    Terminated = 3,
}
