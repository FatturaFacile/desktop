// <copyright file="FieldServiceChoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One option of a combo box: the value kept in the view model and the localized label shown.
/// </summary>
/// <typeparam name="T">The value type.</typeparam>
/// <param name="Value">The value.</param>
/// <param name="Label">The localized label.</param>
internal sealed record FieldServiceChoice<T>(T Value, string Label);
