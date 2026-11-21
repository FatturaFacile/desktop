// <copyright file="MesPanel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The panel open over the operator board.
/// </summary>
internal enum MesPanel
{
    /// <summary>
    /// No panel: the board is fully visible.
    /// </summary>
    None = 0,

    /// <summary>
    /// The suspension reason tiles.
    /// </summary>
    Suspend = 1,

    /// <summary>
    /// The good and scrap pieces keypad.
    /// </summary>
    Declare = 2,

    /// <summary>
    /// The consumed lot (barcode field and quantity keypad).
    /// </summary>
    Lot = 3,

    /// <summary>
    /// The confirmation of closing the phase.
    /// </summary>
    Close = 4,
}
