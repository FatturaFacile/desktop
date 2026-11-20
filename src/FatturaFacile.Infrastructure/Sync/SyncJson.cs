// <copyright file="SyncJson.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;

namespace FatturaFacile.Infrastructure.Sync;

/// <summary>
/// JSON conventions of the web-api (snake_case, lenient numbers).
/// </summary>
internal static class SyncJson
{
    /// <summary>
    /// Gets the serializer options shared by all sync handlers.
    /// </summary>
    internal static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
    };
}
