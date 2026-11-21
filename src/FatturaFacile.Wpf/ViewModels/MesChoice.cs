// <copyright file="MesChoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A touch tile of the operator board: a value (a suspension reason, a defect reason) with its localized label.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <param name="Value">The value.</param>
/// <param name="Label">The localized label.</param>
internal sealed record MesChoice<TValue>(TValue Value, string Label);
