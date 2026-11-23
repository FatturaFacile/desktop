// <copyright file="ProcurementActionResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// The outcome of a procurement write.
/// </summary>
/// <param name="Status">The status.</param>
/// <param name="ErrorKey">The localization key of a refusal.</param>
/// <param name="Detail">The server message of a conflict.</param>
/// <param name="ResultJson">The answer of the server when sent.</param>
public sealed record ProcurementActionResult(
    ProcurementActionStatus Status,
    string? ErrorKey = null,
    string? Detail = null,
    string? ResultJson = null)
{
    /// <summary>
    /// Gets a value indicating whether the write was sent or queued.
    /// </summary>
    public bool IsAccepted => Status is ProcurementActionStatus.Sent or ProcurementActionStatus.Queued;
}
