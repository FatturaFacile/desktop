// <copyright file="ReceivablesChoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// An entry of a receivables filter or selector.
/// </summary>
/// <param name="Key">The key; empty for the entry that means all.</param>
/// <param name="Label">The text shown.</param>
internal sealed record ReceivablesChoice(string Key, string Label);
