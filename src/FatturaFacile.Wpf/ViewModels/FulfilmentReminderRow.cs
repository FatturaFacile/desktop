// <copyright file="FulfilmentReminderRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A reminder of the selected order.
/// </summary>
/// <param name="LevelText">The level.</param>
/// <param name="ChannelText">The channel.</param>
/// <param name="DaysText">The days overdue.</param>
/// <param name="WhenText">When it was sent.</param>
internal sealed record FulfilmentReminderRow(
    string LevelText,
    string ChannelText,
    string DaysText,
    string WhenText);
