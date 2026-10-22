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
    /// Periodic mandatory vehicle roadworthiness test in Italy (<c>Revisione periodica ministeriale</c>) pursuant to the Italian Highway Code (Codice della Strada).
    /// </summary>
    ItalianRevisione = 1,

    /// <summary>
    /// Official Swiss periodic motor vehicle roadworthiness examination (<c>Motorfahrzeugkontrolle / MFK / Contrôle officiel des véhicules automobiles</c>) conducted by cantonal road traffic offices.
    /// </summary>
    SwissMfkCollaudo = 2,
}
