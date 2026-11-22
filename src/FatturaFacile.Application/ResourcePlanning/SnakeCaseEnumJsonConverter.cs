// <copyright file="SnakeCaseEnumJsonConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// Reads and writes an enum as the lower snake case name of the API (<c>not_required</c>, <c>on_target</c>). Any other value is rejected.
/// </summary>
/// <typeparam name="T">The enum.</typeparam>
public sealed class SnakeCaseEnumJsonConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    private static readonly Dictionary<string, T> s_byName = Enum.GetValues<T>().ToDictionary(Name, StringComparer.Ordinal);

    /// <inheritdoc/>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;

        return text is not null && s_byName.TryGetValue(text, out var value)
            ? value
            : throw new JsonException("Unknown " + typeof(T).Name + ": " + (text ?? reader.TokenType.ToString()) + ".");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStringValue(Name(value));
    }

    private static string Name(T value) => JsonNamingPolicy.SnakeCaseLower.ConvertName(value.ToString());
}
