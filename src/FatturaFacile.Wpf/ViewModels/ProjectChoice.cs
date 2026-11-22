// <copyright file="ProjectChoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One choice of a picker: a value and its localized label.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
/// <param name="Value">The value.</param>
/// <param name="Label">The localized label.</param>
internal sealed record ProjectChoice<T>(T Value, string Label);
