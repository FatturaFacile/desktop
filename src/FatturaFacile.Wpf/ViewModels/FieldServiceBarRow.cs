// <copyright file="FieldServiceBarRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A scheduled bar of a technician lane on the timeline.
/// </summary>
/// <param name="Number">The work order number.</param>
/// <param name="Title">The title.</param>
/// <param name="Left">The offset from the left edge of the timeline, in device-independent pixels.</param>
/// <param name="Width">The width of the bar, in device-independent pixels.</param>
/// <param name="Status">The raw status, for the colour.</param>
/// <param name="Tooltip">The tooltip with the window, the status and the site.</param>
internal sealed record FieldServiceBarRow(string Number, string Title, double Left, double Width, string Status, string Tooltip);
