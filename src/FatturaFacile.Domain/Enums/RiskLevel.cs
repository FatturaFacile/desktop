// <copyright file="RiskLevel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies the workplace health and safety risk category or credit/compliance risk level.
/// </summary>
public enum RiskLevel
{

    /// <summary>
    /// Minimal occupational hazard or credit exposure, requiring standard procedural precautions and baseline monitoring.
    /// </summary>
    Low = 1,

    /// <summary>
    /// Moderate occupational hazard (e.g., light assembly, warehouse machinery) or commercial risk, requiring active surveillance, routine inspections, and targeted prevention plans.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// High-hazard occupational profile (e.g., heavy construction, working at height, hazardous substances) or severe exposure risk, mandating rigorous safety certifications and continuous oversight.
    /// </summary>
    High = 3,

}
