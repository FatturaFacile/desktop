// <copyright file="HelpdeskRecentTicket.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// A recent ticket of the same customer.
/// </summary>
/// <param name="Id">The server identifier.</param>
/// <param name="Number">The ticket number.</param>
/// <param name="Subject">The subject.</param>
/// <param name="Status">The status.</param>
public sealed record HelpdeskRecentTicket(Guid? Id, string Number, string Subject, string Status);
