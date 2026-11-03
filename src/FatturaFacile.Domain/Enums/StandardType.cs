// <copyright file="StandardType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies the engineering and construction bill of quantities (BoQ) classification standard used for itemized estimates and tenders.
/// </summary>
public enum StandardType
{

    /// <summary>
    /// Italian construction cost estimation and bill of quantities format (Computo Metrico Estimativo) based on regional price lists (Prezzari Regionali).
    /// </summary>
    ItalianComputo = 1,

    /// <summary>
    /// Swiss construction documentation standard published by the Swiss Society of Engineers and Architects (SIA)
    /// and the Swiss Centre for Construction Rationalisation (CRB) utilizing the Standardised Item Catalogue
    /// (Normpositionen-Katalog / NPK / CAN).
    /// </summary>
    SwissSiaCrbNpk = 2,

}
