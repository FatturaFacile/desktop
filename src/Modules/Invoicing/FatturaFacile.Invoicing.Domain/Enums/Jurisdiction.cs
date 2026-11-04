// <copyright file="Jurisdiction.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Invoicing.Domain.Enums;

/// <summary>
/// Represents the fiscal jurisdiction and compliance territory.
/// </summary>
public enum Jurisdiction
{
    /// <summary>
    /// Italian Republic jurisdiction (Agenzia delle Entrate, SDI, Codice Fiscale/Partita IVA).
    /// </summary>
    IT = 1,

    /// <summary>
    /// Swiss Confederation jurisdiction (Federal Tax Administration FTA, QR-bill, CHF rounding).
    /// </summary>
    CH = 2,
}
