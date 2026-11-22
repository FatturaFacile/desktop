// <copyright file="InventoryCountingResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.InventoryCounting;

/// <summary>
/// The outcome of a call to the physical inventory web-api surface; failures are returned, never thrown.
/// </summary>
/// <typeparam name="T">The type of the value of a successful call.</typeparam>
public sealed record InventoryCountingResult<T>
{
    /// <summary>
    /// Gets the value of a successful call.
    /// </summary>
    public T? Value { get; init; }

    /// <summary>
    /// Gets the HTTP status code, or zero when the server could not be reached.
    /// </summary>
    public int StatusCode { get; init; }

    /// <summary>
    /// Gets the server message of a failed call.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the validation messages of a 422 answer.
    /// </summary>
    public IReadOnlyList<string> Errors { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the call succeeded.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets a value indicating whether the server could not be reached.
    /// </summary>
    public bool IsOffline => !IsSuccess && StatusCode == 0;

    /// <summary>
    /// Gets a value indicating whether the user lacks the permission for the call.
    /// </summary>
    public bool IsForbidden => !IsSuccess && StatusCode == 403;

    /// <summary>
    /// Gets a value indicating whether the server rejected the input (HTTP 422).
    /// </summary>
    public bool IsValidationError => !IsSuccess && StatusCode == 422;
}
