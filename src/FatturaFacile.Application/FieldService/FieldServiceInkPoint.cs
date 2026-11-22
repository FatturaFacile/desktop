// <copyright file="FieldServiceInkPoint.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// A sampled point of an ink stroke, in device-independent pixels of the signature pad.
/// </summary>
/// <param name="X">The horizontal position.</param>
/// <param name="Y">The vertical position.</param>
public sealed record FieldServiceInkPoint(double X, double Y);
