// <copyright file="BezugsteuerCategory.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Foreign service categories subject to Swiss Bezugsteuer (Art. 45 LIVA).
/// </summary>
public enum BezugsteuerCategory
{
    /// <summary>
    /// Software, SaaS, and cloud licenses.
    /// </summary>
    SoftwareCloud = 0,

    /// <summary>
    /// Consulting and advisory services.
    /// </summary>
    Consulting = 1,

    /// <summary>
    /// IT engineering and development services.
    /// </summary>
    ItServices = 2,

    /// <summary>
    /// Telecommunications services.
    /// </summary>
    Telecom = 3,

    /// <summary>
    /// Royalties and intellectual property licenses.
    /// </summary>
    Royalties = 4,

    /// <summary>
    /// Online advertising and marketing services.
    /// </summary>
    Marketing = 5,

    /// <summary>
    /// Other cross-border services.
    /// </summary>
    OtherCrossBorderService = 6,

    /// <summary>
    /// Imported services.
    /// </summary>
    ImportService = 7,
}
