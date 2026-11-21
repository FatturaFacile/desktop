// <copyright file="CopilotCacheSerializer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// Serializes the structured parts of a message (citations, visualization, proposed action) for the local SQLite cache.
/// </summary>
public static class CopilotCacheSerializer
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    /// <summary>
    /// Serializes a value to JSON.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>The JSON text.</returns>
    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    /// <summary>
    /// Deserializes a value, returning the default when the text is empty or corrupt.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="json">The JSON text.</param>
    /// <returns>The value, or the default.</returns>
    public static T? Deserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static JsonSerializerOptions CreateOptions()
    {
        JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
