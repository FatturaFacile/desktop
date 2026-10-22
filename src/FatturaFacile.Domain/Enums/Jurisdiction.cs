// <copyright file="Jurisdiction.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies the tax, labor, and legal jurisdiction governing commercial transactions, fiscal document formats, and labor compliance.
/// </summary>
public enum Jurisdiction
{

    /// <summary>
    /// Italian legal and fiscal jurisdiction, governed by the Italian Tax Authority (<c>Agenzia delle Entrate</c>),
    /// electronic invoicing system (<c>SDI</c>), and national labor regulations (INPS/INAIL).
    /// </summary>
    Italy = 1,

    /// <summary>
    /// Swiss Confederation legal and fiscal jurisdiction, governed by the Federal Tax Administration (ESTV/AFC),
    /// Swiss QR-bill standards, and cantonal labor/social security schemes (AHV/AVS, ALV/AC).
    /// </summary>
    Switzerland = 2,

    /// <summary>
    /// Cross-border bilateral regime between Italy and Switzerland, addressing international trade,
    /// customs declarations, cross-border frontier workers (<c>frontalieri</c>), and dual-treaty tax conventions.
    /// </summary>
    CrossBorder = 3,

}
