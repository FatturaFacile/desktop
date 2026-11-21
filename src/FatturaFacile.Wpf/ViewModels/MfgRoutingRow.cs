// <copyright file="MfgRoutingRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A routing row of the manufacturing view.
/// </summary>
/// <param name="Id">The routing identifier.</param>
/// <param name="Number">The routing number.</param>
/// <param name="Revision">The revision.</param>
/// <param name="Item">The manufactured item.</param>
/// <param name="Name">The name.</param>
/// <param name="Status">The status.</param>
/// <param name="StepCount">The number of operation steps.</param>
/// <param name="Default">The localized default flag.</param>
internal sealed record MfgRoutingRow(Guid Id, string Number, string Revision, string Item, string Name, string Status, int StepCount, string Default);
