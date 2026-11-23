// <copyright file="ReceivablesQueueRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A queued or refused receivables action.
/// </summary>
/// <param name="Id">The entry identifier.</param>
/// <param name="ActionText">The action.</param>
/// <param name="Reference">The label of the action.</param>
/// <param name="WhenText">When the user acted.</param>
/// <param name="Error">The reason the server gave.</param>
internal sealed record ReceivablesQueueRow(Guid Id, string ActionText, string Reference, string WhenText, string? Error);
