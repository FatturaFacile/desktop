// <copyright file="BaselineSnapshotSerializer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// Reads and writes the JSON of a baseline snapshot (snake_case, enums as text), the shape shared with the web-api.
/// </summary>
public static class BaselineSnapshotSerializer
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) },
    };

    /// <summary>
    /// Serializes a snapshot.
    /// </summary>
    /// <param name="snapshot">The snapshot.</param>
    /// <returns>The JSON text.</returns>
    public static string Serialize(BaselineSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return JsonSerializer.Serialize(snapshot, s_options);
    }

    /// <summary>
    /// Reads a snapshot.
    /// </summary>
    /// <param name="json">The JSON text.</param>
    /// <returns>The snapshot, or null when the text is not a snapshot.</returns>
    public static BaselineSnapshot? Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<BaselineSnapshot>(json, s_options);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
