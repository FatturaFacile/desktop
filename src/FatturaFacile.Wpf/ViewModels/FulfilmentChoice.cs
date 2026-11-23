// <copyright file="FulfilmentChoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A code with its label for a combo box.
/// </summary>
/// <param name="Code">The wire code, null for all.</param>
/// <param name="Label">The localized label.</param>
internal sealed record FulfilmentChoice(
    string? Code,
    string Label);
