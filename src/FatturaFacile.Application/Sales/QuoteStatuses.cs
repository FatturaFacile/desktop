// <copyright file="QuoteStatuses.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Sales;

/// <summary>
/// Workflow statuses of a sales quote (same vocabulary as web-api).
/// </summary>
public static class QuoteStatuses
{
    /// <summary>
    /// The "draft" status.
    /// </summary>
    public const string Draft = "draft";

    /// <summary>
    /// The "pending_approval" status.
    /// </summary>
    public const string PendingApproval = "pending_approval";

    /// <summary>
    /// The "approved" status.
    /// </summary>
    public const string Approved = "approved";

    /// <summary>
    /// The "sent" status.
    /// </summary>
    public const string Sent = "sent";

    /// <summary>
    /// The "accepted" status.
    /// </summary>
    public const string Accepted = "accepted";

    /// <summary>
    /// The "rejected" status.
    /// </summary>
    public const string Rejected = "rejected";

    /// <summary>
    /// The "expired" status.
    /// </summary>
    public const string Expired = "expired";

    /// <summary>
    /// The "superseded" status.
    /// </summary>
    public const string Superseded = "superseded";
}
