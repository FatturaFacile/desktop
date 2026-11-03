// <copyright file="InspectionType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies the regulatory or statutory vehicle roadworthiness inspection standard applied according to jurisdiction.
/// </summary>
public enum InspectionType
{

    /// <summary>
    /// Periodic mandatory vehicle roadworthiness test in Italy (Revisione periodica ministeriale) pursuant to the Italian Highway Code (Codice della Strada).
    /// </summary>
    ItalianRevisione = 1,

    /// <summary>
    /// Official Swiss periodic motor vehicle roadworthiness examination
    /// (Motorfahrzeugkontrolle / MFK / Contrôle officiel des véhicules automobiles) conducted by cantonal road traffic offices.
    /// </summary>
    SwissMfkCollaudo = 2,

}
