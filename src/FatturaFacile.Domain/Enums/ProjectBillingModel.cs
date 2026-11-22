// <copyright file="ProjectBillingModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// How a project is billed (Mod 078).
/// </summary>
public enum ProjectBillingModel
{
    /// <summary>
    /// Billed on hours and materials.
    /// </summary>
    TimeAndMaterials = 0,

    /// <summary>
    /// Billed a fixed price.
    /// </summary>
    FixedPrice = 1,

    /// <summary>
    /// Billed on progress statements (SAL).
    /// </summary>
    SalProgress = 2,

    /// <summary>
    /// Billed a recurring retainer.
    /// </summary>
    Retainer = 3,
}
