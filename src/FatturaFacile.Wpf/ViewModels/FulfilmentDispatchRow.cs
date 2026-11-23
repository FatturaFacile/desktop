// <copyright file="FulfilmentDispatchRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A dispatch of the selected order.
/// </summary>
/// <param name="ChannelText">The channel.</param>
/// <param name="RecipientText">The recipient.</param>
/// <param name="StatusText">The status.</param>
/// <param name="WhenText">When it was dispatched.</param>
internal sealed record FulfilmentDispatchRow(
    string ChannelText,
    string RecipientText,
    string StatusText,
    string WhenText);
