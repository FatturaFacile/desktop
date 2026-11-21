// <copyright file="DssChoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A value of a filter list and the label shown for it.
/// </summary>
/// <param name="Value">The value sent to the server (for example "YTD").</param>
/// <param name="Label">The localized label.</param>
internal sealed record DssChoice(string Value, string Label);
