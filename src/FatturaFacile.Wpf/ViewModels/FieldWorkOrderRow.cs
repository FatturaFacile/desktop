// <copyright file="FieldWorkOrderRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A work order of the work order picker, formatted for display.
/// </summary>
/// <param name="Id">The work order identifier.</param>
/// <param name="Number">The number.</param>
/// <param name="Title">The title.</param>
/// <param name="StatusText">The localized status.</param>
/// <param name="TypeText">The localized type.</param>
/// <param name="WindowText">The scheduled window.</param>
internal sealed record FieldWorkOrderRow(string Id, string Number, string Title, string StatusText, string TypeText, string WindowText);
