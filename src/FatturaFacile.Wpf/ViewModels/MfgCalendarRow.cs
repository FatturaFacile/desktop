// <copyright file="MfgCalendarRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A working calendar row of the manufacturing view.
/// </summary>
/// <param name="Code">The code.</param>
/// <param name="Name">The name.</param>
/// <param name="Timezone">The time zone.</param>
/// <param name="Shifts">The weekly shifts summary.</param>
/// <param name="Closures">The number of closures.</param>
internal sealed record MfgCalendarRow(string Code, string Name, string Timezone, string Shifts, string Closures);
