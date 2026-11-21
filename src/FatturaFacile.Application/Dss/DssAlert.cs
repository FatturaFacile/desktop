// <copyright file="DssAlert.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Dss;

/// <summary>
/// A strategic alert.
/// </summary>
/// <param name="Code">The alert code (for example "margin_erosion").</param>
/// <param name="Severity">CRITICAL, WARNING or INFO.</param>
/// <param name="Metric">The metric the alert is about.</param>
/// <param name="Value">The offending value.</param>
/// <param name="Threshold">The threshold.</param>
/// <param name="Label">The customer or entity the alert names, when it names one.</param>
public sealed record DssAlert(string Code, string Severity, string Metric, double Value, double Threshold, string? Label);
