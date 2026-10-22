// <copyright file="DamageSeverity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies the severity tier of vehicle, equipment, or property damage identified during maintenance check-ins, field inspections, or claims assessment.
/// </summary>
public enum DamageSeverity
{

    /// <summary>
    /// Superficial, cosmetic damage (e.g., light scratches, chipped paint, minor scuffs) that does not impair structural integrity, safe operation, or functional compliance.
    /// </summary>
    Minor = 1,

    /// <summary>
    /// Substantial damage (e.g., body panel dents, cracked windshields, worn operational components)
    /// requiring workshop repair or parts replacement, but not immediately rendering the asset unroadworthy.
    /// </summary>
    Moderate = 2,

    /// <summary>
    /// Critical or safety-compromising damage (e.g., frame or chassis deformation, steering/brake system failure,
    /// deployed airbags) requiring immediate decommissioning and major structural overhaul.
    /// </summary>
    Severe = 3,

}
