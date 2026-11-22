// <copyright file="HelpdeskActionResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// The outcome of a helpdesk action.
/// </summary>
/// <param name="Status">What became of the action.</param>
/// <param name="ErrorKey">The localization key of the reason when local validation rejected it.</param>
/// <param name="Detail">The message of the server when it refused the action.</param>
/// <param name="TimeOutcome">For a time log: how it was settled (the server answer, or an estimate while queued).</param>
/// <param name="TrackingToken">For an RMA: the tracking token of the customer.</param>
public sealed record HelpdeskActionResult(
    HelpdeskActionStatus Status,
    string? ErrorKey = null,
    string? Detail = null,
    HelpdeskTimeOutcome? TimeOutcome = null,
    string? TrackingToken = null);
