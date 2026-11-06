// <copyright file="QrBillPathFigure.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Reporting;

/// <summary>
/// A sub-path of an SVG path: its points in millimetres and whether it is closed.
/// </summary>
public sealed class QrBillPathFigure
{
    /// <summary>
    /// Gets the points of the figure.
    /// </summary>
    public IList<(float X, float Y)> Points { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the figure is closed.
    /// </summary>
    public bool Closed { get; set; }
}
