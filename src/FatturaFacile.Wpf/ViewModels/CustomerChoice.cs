// <copyright file="CustomerChoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One entry of the customer picker.
/// </summary>
/// <param name="Id">The contact identifier.</param>
/// <param name="Label">The display name.</param>
internal sealed record CustomerChoice(Guid Id, string Label);
