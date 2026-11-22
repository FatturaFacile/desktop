// <copyright file="ProjectEvmLight.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectEvm;

/// <summary>
/// Traffic light of a performance index: green from 1.00, amber from 0.90, red below.
/// </summary>
public enum ProjectEvmLight
{
    /// <summary>
    /// On track (the index is at least 1.00, or undefined).
    /// </summary>
    Green = 0,

    /// <summary>
    /// To watch (the index is from 0.90 to below 1.00).
    /// </summary>
    Amber = 1,

    /// <summary>
    /// Critical (the index is below 0.90).
    /// </summary>
    Red = 2,
}
