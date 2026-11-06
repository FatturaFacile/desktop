// <copyright file="SdiStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Transmission status for the Italian Sistema di Interscambio (SDI) electronic invoice.
/// </summary>
public enum SdiStatus
{
    /// <summary>
    /// Document is drafted, not yet validated or queued for transmission.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// XML FatturaPA document validated against schema.
    /// </summary>
    Validated = 1,

    /// <summary>
    /// Sent to SDI transmission channel.
    /// </summary>
    Sent = 2,

    /// <summary>
    /// Delivered to recipient (Ricevuta di consegna).
    /// </summary>
    Delivered = 3,

    /// <summary>
    /// Rejected by SDI (Notifica di scarto).
    /// </summary>
    Rejected = 4,
}
