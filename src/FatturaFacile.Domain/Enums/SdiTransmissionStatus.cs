// <copyright file="SdiTransmissionStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Defines the transmission lifecycle status of an electronic invoice through the Italian Revenue Agency's Exchange System (Sistema di Interscambio / SDI).
/// </summary>
public enum SdiTransmissionStatus
{

    /// <summary>
    /// Document is in internal draft state and has not been validated or prepared for electronic dispatch.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Invoice XML structure and digital signature have been validated, queued, and are awaiting dispatch to the SDI gateway.
    /// </summary>
    ReadyToSend = 1,

    /// <summary>
    /// File has been transmitted to SDI and the system is awaiting an official transmission receipt or outcome notification (Ricevuta di Consegna or Notifica di Mancata Consegna).
    /// </summary>
    Sent = 2,

    /// <summary>
    /// Invoice was successfully validated by SDI and acknowledged as delivered to the recipient's certified email (PEC) or SDI channel (Ricevuta di Consegna).
    /// </summary>
    Delivered = 3,

    /// <summary>
    /// Technical transmission failure occurred prior to or during dispatch to SDI (e.g., connectivity loss, web service timeout, or digital signing fault).
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Invoice was parsed and rejected by SDI (Notifica di Scarto) due to schema non-compliance, invalid VAT/tax numbers, duplicate document numbers, or failed integrity checks.
    /// </summary>
    Rejected = 5,

}
