// <copyright file="CustomerTimelineRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One touchpoint of the customer timeline, formatted for display.
/// </summary>
/// <param name="WhenText">The formatted date and time.</param>
/// <param name="TypeText">The localized touchpoint type.</param>
/// <param name="Title">The title.</param>
/// <param name="StatusText">The status, or an empty text.</param>
/// <param name="AmountText">The amount, or an empty text.</param>
internal sealed record CustomerTimelineRow(string WhenText, string TypeText, string Title, string StatusText, string AmountText);
