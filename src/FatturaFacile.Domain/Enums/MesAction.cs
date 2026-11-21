// <copyright file="MesAction.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// An operator action on a phase of the MES operator board.
/// </summary>
public enum MesAction
{
    /// <summary>
    /// Takes the phase over (presa in carico), or hands it to another operator.
    /// </summary>
    TakeOver = 0,

    /// <summary>
    /// Starts the setup (avvio setup).
    /// </summary>
    StartSetup = 1,

    /// <summary>
    /// Starts the machining (inizio lavorazione).
    /// </summary>
    StartProduction = 2,

    /// <summary>
    /// Suspends the phase with a reason (sospensione).
    /// </summary>
    Suspend = 3,

    /// <summary>
    /// Resumes a suspended phase (ripresa).
    /// </summary>
    Resume = 4,

    /// <summary>
    /// Declares good and scrapped pieces.
    /// </summary>
    DeclareProduction = 5,

    /// <summary>
    /// Registers a component lot consumed.
    /// </summary>
    ConsumeMaterial = 6,

    /// <summary>
    /// Closes the phase (chiusura fase).
    /// </summary>
    Close = 7,
}
