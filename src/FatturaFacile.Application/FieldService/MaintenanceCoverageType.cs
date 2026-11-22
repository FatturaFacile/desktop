// <copyright file="MaintenanceCoverageType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// What a maintenance contract covers; it decides which lines of a service report are charged to the customer.
/// </summary>
public enum MaintenanceCoverageType
{
    /// <summary>
    /// Labor is covered, parts are charged (`labor_only`).
    /// </summary>
    LaborOnly = 0,

    /// <summary>
    /// Parts and labor are covered (`parts_and_labor`).
    /// </summary>
    PartsAndLabor = 1,

    /// <summary>
    /// Only the scheduled preventive visits are covered (`preventive_only`).
    /// </summary>
    PreventiveOnly = 2,

    /// <summary>
    /// Everything, including travel and call-out, is covered (`full_comprehensive`).
    /// </summary>
    FullComprehensive = 3,
}
