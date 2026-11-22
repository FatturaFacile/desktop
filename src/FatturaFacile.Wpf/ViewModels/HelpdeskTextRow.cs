// <copyright file="HelpdeskTextRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A line of a side panel list with a main text and a detail.
/// </summary>
/// <param name="Text">The main text.</param>
/// <param name="Detail">The detail.</param>
internal sealed record HelpdeskTextRow(string Text, string Detail);
