// <copyright file="ChoiceItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.PhysicalInventory;

/// <summary>
/// A value of a drop-down with its localized label.
/// </summary>
/// <param name="Value">The value sent to the web-api.</param>
/// <param name="Label">The localized label.</param>
internal sealed record ChoiceItem(string Value, string Label);
