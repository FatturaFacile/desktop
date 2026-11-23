// <copyright file="DispatchView.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// One transmission of a purchase order to its supplier.
/// </summary>
/// <param name="Id">The dispatch identifier.</param>
/// <param name="Channel">The channel (pdf, email, edi, peppol).</param>
/// <param name="Status">The status.</param>
/// <param name="Recipient">The recipient.</param>
/// <param name="MessageReference">The reference of the message.</param>
/// <param name="DispatchedAt">When it was dispatched (UTC).</param>
public sealed record DispatchView(
    Guid Id,
    string Channel,
    string Status,
    string? Recipient,
    string? MessageReference,
    DateTime DispatchedAt);
