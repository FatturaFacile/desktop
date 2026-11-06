// <copyright file="SdiNotification.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Invoicing.Sdi;

/// <summary>
/// A notification of the Sistema di Interscambio about a sent invoice.
/// </summary>
public sealed class SdiNotification
{
    /// <summary>
    /// Gets or sets the type (RC, NS, MC, NE, DT, AT).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SDI identifier of the invoice.
    /// </summary>
    public string? SdiIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the name of the invoice file the notification refers to.
    /// </summary>
    public string? InvoiceFileName { get; set; }

    /// <summary>
    /// Gets or sets the date of delivery or reception.
    /// </summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the outcome of the public administration (EC01 accepted, EC02 refused).
    /// </summary>
    public string? Outcome { get; set; }

    /// <summary>
    /// Gets the errors of a rejection.
    /// </summary>
    public IList<string> Errors { get; } = [];

    /// <summary>
    /// Gets the invoice state resulting from this notification.
    /// </summary>
    public string? ResultingStatus => Type switch
    {
        "RC" => SdiStatus.Delivered,
        "NS" => SdiStatus.Rejected,
        "MC" => SdiStatus.NotDelivered,
        "NE" => string.Equals(Outcome, "EC02", StringComparison.OrdinalIgnoreCase) ? SdiStatus.Refused : SdiStatus.Accepted,
        "DT" => SdiStatus.TermsExpired,
        "AT" => SdiStatus.Attested,
        _ => null,
    };
}
