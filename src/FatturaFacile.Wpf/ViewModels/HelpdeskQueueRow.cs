// <copyright file="HelpdeskQueueRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// An action waiting for the server, or refused by it, in the queue list of the workbench.
/// </summary>
/// <param name="Id">The outbox entry.</param>
/// <param name="ActionText">The localized action.</param>
/// <param name="TicketNumber">The ticket number.</param>
/// <param name="WhenText">The formatted instant the user acted.</param>
/// <param name="Error">The reason given by the server, for a refused action.</param>
internal sealed record HelpdeskQueueRow(Guid Id, string ActionText, string TicketNumber, string WhenText, string? Error);
