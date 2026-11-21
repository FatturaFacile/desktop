// <copyright file="MfgLossRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One of the six big losses of a work centre.
/// </summary>
/// <param name="Label">The localized loss name.</param>
/// <param name="Factor">The localized OEE factor.</param>
/// <param name="Minutes">The lost minutes text.</param>
internal sealed record MfgLossRow(string Label, string Factor, string Minutes);
