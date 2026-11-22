// <copyright file="FieldServiceCanvasPoint.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// A position on the map canvas, in device-independent pixels from its top-left corner.
/// </summary>
/// <param name="X">The horizontal position.</param>
/// <param name="Y">The vertical position.</param>
public sealed record FieldServiceCanvasPoint(double X, double Y);
