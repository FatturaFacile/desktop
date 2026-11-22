// <copyright file="HelpdeskChoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// An entry of a filter list of the helpdesk views: a stable key with its localized label.
/// </summary>
/// <param name="Key">The key; the all entry has an empty key.</param>
/// <param name="Label">The localized label.</param>
internal sealed record HelpdeskChoice(string Key, string Label);
