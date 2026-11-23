// <copyright file="FulfilmentQueueRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A queued write of the outbox.
/// </summary>
/// <param name="Id">The entry.</param>
/// <param name="ActionText">The action.</param>
/// <param name="Reference">The label.</param>
/// <param name="WhenText">When the user acted.</param>
/// <param name="ErrorText">The last error.</param>
internal sealed record FulfilmentQueueRow(
    Guid Id,
    string ActionText,
    string Reference,
    string WhenText,
    string ErrorText);
