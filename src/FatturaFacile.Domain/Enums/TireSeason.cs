// <copyright file="TireSeason.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies the seasonal classification and operating compound of vehicle tires for workshop maintenance and statutory tire swap management.
/// </summary>
public enum TireSeason
{

    /// <summary>
    /// Summer tires formulated for warm conditions, providing optimized grip, low rolling resistance, and hydroplaning resistance above 7 degrees Celsius.
    /// </summary>
    Summer = 1,

    /// <summary>
    /// Winter tires featuring specialized cold-weather rubber compounds and siped tread blocks (3PMSF / M+S marked), mandated during winter road equipment enforcement windows.
    /// </summary>
    Winter = 2,

    /// <summary>
    /// All-season hybrid tires certified for year-round operational compliance across moderate climate conditions.
    /// </summary>
    AllSeason = 3,

}
