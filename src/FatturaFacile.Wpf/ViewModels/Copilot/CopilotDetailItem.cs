// <copyright file="CopilotDetailItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.Copilot;

/// <summary>
/// A fact shown on an action confirmation card.
/// </summary>
/// <param name="Label">The label.</param>
/// <param name="Value">The value.</param>
internal sealed record CopilotDetailItem(string Label, string Value);
