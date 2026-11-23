// <copyright file="ProcurementCodes.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// The wire codes of the purchase order fulfilment (Mod 046): order statuses, dispatch channels and dropshipment statuses, with the
/// rules of which actions an order allows.
/// </summary>
public static class ProcurementCodes
{
    /// <summary>
    /// The status of an order approved and not yet sent.
    /// </summary>
    public const string Approved = "approved";

    /// <summary>
    /// The status of an order sent to the supplier.
    /// </summary>
    public const string Sent = "sent";

    /// <summary>
    /// The status of an order the supplier confirmed.
    /// </summary>
    public const string Confirmed = "confirmed";

    /// <summary>
    /// The status of an order partially received.
    /// </summary>
    public const string PartiallyReceived = "partially_received";

    /// <summary>
    /// The dispatch channel PDF.
    /// </summary>
    public const string ChannelPdf = "pdf";

    /// <summary>
    /// The dispatch channel email.
    /// </summary>
    public const string ChannelEmail = "email";

    /// <summary>
    /// The dispatch channel EDI.
    /// </summary>
    public const string ChannelEdi = "edi";

    /// <summary>
    /// The dispatch channel Peppol.
    /// </summary>
    public const string ChannelPeppol = "peppol";

    /// <summary>
    /// The dropshipment status before the transport document.
    /// </summary>
    public const string DropshipPending = "pending";

    /// <summary>
    /// The dropshipment status once the transport document was issued.
    /// </summary>
    public const string DropshipDdtIssued = "ddt_issued";

    /// <summary>
    /// The dropshipment status of a partial delivery.
    /// </summary>
    public const string DropshipPartiallyDelivered = "partially_delivered";

    /// <summary>
    /// The dropshipment status of a complete delivery.
    /// </summary>
    public const string DropshipDelivered = "delivered";

    /// <summary>
    /// Gets the order statuses the order list can filter by.
    /// </summary>
    public static IReadOnlyList<string> OrderStatuses { get; } =
        ["draft", "pending_approval", Approved, "rejected", Sent, Confirmed, PartiallyReceived, "received", "closed", "cancelled"];

    /// <summary>
    /// Gets the dispatch channels.
    /// </summary>
    public static IReadOnlyList<string> Channels { get; } = [ChannelPdf, ChannelEmail, ChannelEdi, ChannelPeppol];

    /// <summary>
    /// Gets the dropshipment statuses.
    /// </summary>
    public static IReadOnlyList<string> DropshipStatuses { get; } = [DropshipPending, DropshipDdtIssued, DropshipPartiallyDelivered, DropshipDelivered];

    /// <summary>
    /// Tells whether an order of the status can be transmitted (an approved order, or a sent one to send again).
    /// </summary>
    /// <param name="status">The wire status.</param>
    /// <returns>True when the order can be dispatched.</returns>
    public static bool CanDispatch(string status) =>
        string.Equals(status, Approved, StringComparison.Ordinal) || string.Equals(status, Sent, StringComparison.Ordinal);

    /// <summary>
    /// Tells whether the supplier confirmation of an order of the status can be recorded.
    /// </summary>
    /// <param name="status">The wire status.</param>
    /// <returns>True when a confirmation can be recorded.</returns>
    public static bool CanConfirm(string status) =>
        string.Equals(status, Sent, StringComparison.Ordinal) || string.Equals(status, Confirmed, StringComparison.Ordinal);

    /// <summary>
    /// Tells whether a dropshipment of the status can still receive its transport document.
    /// </summary>
    /// <param name="status">The wire status.</param>
    /// <returns>True when the document can be issued.</returns>
    public static bool CanIssueDdt(string status) => string.Equals(status, DropshipPending, StringComparison.Ordinal);

    /// <summary>
    /// Tells whether a dropshipment of the status can still receive a delivery.
    /// </summary>
    /// <param name="status">The wire status.</param>
    /// <returns>True when a delivery can be recorded.</returns>
    public static bool CanDeliver(string status) =>
        string.Equals(status, DropshipDdtIssued, StringComparison.Ordinal) || string.Equals(status, DropshipPartiallyDelivered, StringComparison.Ordinal);
}
