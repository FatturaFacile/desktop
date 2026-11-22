// <copyright file="ReceivablesActionResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// The result of a receivables action.
/// </summary>
/// <param name="Status">What became of the action.</param>
/// <param name="ErrorKey">The localization key of the local validation problem.</param>
/// <param name="Detail">The reason the server gave when it refused the action.</param>
/// <param name="ResultJson">The body of the server answer when it accepted the action.</param>
public sealed record ReceivablesActionResult(
    ReceivablesActionStatus Status,
    string? ErrorKey = null,
    string? Detail = null,
    string? ResultJson = null)
{
    /// <summary>
    /// Gets a value indicating whether the action was sent or queued.
    /// </summary>
    public bool IsAccepted => Status is ReceivablesActionStatus.Sent or ReceivablesActionStatus.Queued;
}
