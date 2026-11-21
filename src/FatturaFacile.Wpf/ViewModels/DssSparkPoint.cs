// <copyright file="DssSparkPoint.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A point of a KPI trend line.
/// </summary>
/// <param name="Index">The position in the series.</param>
/// <param name="Value">The value.</param>
internal sealed record DssSparkPoint(int Index, double Value);
