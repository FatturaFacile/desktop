// <copyright file="FieldServiceInkSketch.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// What the customer drew on the signature pad: the strokes and the size of the pad they were drawn on.
/// </summary>
/// <param name="Strokes">The strokes, each one the sampled points of a pen-down to pen-up movement.</param>
/// <param name="Width">The width of the pad.</param>
/// <param name="Height">The height of the pad.</param>
public sealed record FieldServiceInkSketch(IReadOnlyList<IReadOnlyList<FieldServiceInkPoint>> Strokes, double Width, double Height);
