// <copyright file="ContactOption.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.VatCompliance;

/// <summary>
/// A customer or supplier in a picker.
/// </summary>
/// <param name="Id">The id.</param>
/// <param name="Label">The label.</param>
internal sealed record ContactOption(Guid Id, string Label);
