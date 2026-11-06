// <copyright file="SdiStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Invoicing.Sdi;

/// <summary>
/// States of an electronic invoice in the Sistema di Interscambio.
/// </summary>
public static class SdiStatus
{
    /// <summary>
    /// Sent, waiting for the receipt.
    /// </summary>
    public const string Sent = "Inviata";

    /// <summary>
    /// Delivered to the recipient (RC).
    /// </summary>
    public const string Delivered = "Consegnata";

    /// <summary>
    /// Rejected by the SDI (NS): a corrected file must be sent.
    /// </summary>
    public const string Rejected = "Scartata";

    /// <summary>
    /// Not delivered (MC): the invoice is valid and available to the recipient in the tax portal.
    /// </summary>
    public const string NotDelivered = "MancataConsegna";

    /// <summary>
    /// Accepted by the public administration (NE EC01).
    /// </summary>
    public const string Accepted = "Accettata";

    /// <summary>
    /// Refused by the public administration (NE EC02).
    /// </summary>
    public const string Refused = "Rifiutata";

    /// <summary>
    /// No answer of the public administration within 15 days (DT).
    /// </summary>
    public const string TermsExpired = "DecorrenzaTermini";

    /// <summary>
    /// Delivery impossible, transmission attested (AT).
    /// </summary>
    public const string Attested = "ImpossibilitaRecapito";

    private static readonly string[] Locked = [Sent, Delivered, NotDelivered, Accepted, TermsExpired, Attested];

    /// <summary>
    /// Gets a value indicating whether an invoice in this state may be sent (again).
    /// </summary>
    /// <param name="status">The current state.</param>
    /// <returns><see langword="true"/> when sending is allowed.</returns>
    public static bool CanSend(string? status) =>
        !Locked.Contains(status ?? string.Empty, StringComparer.OrdinalIgnoreCase);
}
