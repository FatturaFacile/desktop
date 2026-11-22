// <copyright file="HelpdeskCustomerContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// The customer 360 panel of a ticket: contracts and hour banks, assets and warranty, open invoices and recent tickets.
/// </summary>
/// <param name="CustomerId">The customer, when known.</param>
/// <param name="Contracts">The service contracts.</param>
/// <param name="Assets">The assets.</param>
/// <param name="OpenInvoices">The open invoices.</param>
/// <param name="RecentTickets">The recent tickets.</param>
public sealed record HelpdeskCustomerContext(
    Guid? CustomerId,
    IReadOnlyList<HelpdeskContract> Contracts,
    IReadOnlyList<HelpdeskAsset> Assets,
    HelpdeskOpenInvoices OpenInvoices,
    IReadOnlyList<HelpdeskRecentTicket> RecentTickets)
{
    /// <summary>
    /// Gets the hours left on the contracts that have an hour bank, or null when none has one.
    /// </summary>
    public decimal? RemainingContractHours => Contracts.Any(contract => contract.RemainingHours is not null)
        ? Contracts.Sum(contract => contract.RemainingHours ?? 0m)
        : null;
}
