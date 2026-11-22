// <copyright file="InventoryCountingResults.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.InventoryCounting;

/// <summary>
/// Factory of <see cref="InventoryCountingResult{T}"/> values.
/// </summary>
public static class InventoryCountingResults
{
    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>The result.</returns>
    public static InventoryCountingResult<T> Ok<T>(T value, int statusCode = 200) => new() { IsSuccess = true, Value = value, StatusCode = statusCode };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="statusCode">The HTTP status code, or zero when offline.</param>
    /// <param name="message">The server message.</param>
    /// <param name="errors">The validation messages.</param>
    /// <returns>The result.</returns>
    public static InventoryCountingResult<T> Fail<T>(int statusCode, string message, IReadOnlyList<string>? errors = null) =>
        new() { IsSuccess = false, StatusCode = statusCode, Message = message, Errors = errors ?? [] };
}
