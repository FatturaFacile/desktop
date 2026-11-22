// <copyright file="ResourceBookingTypeJsonConverter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// Reads and writes the booking type with the exact names of the API: <c>SoftTentative</c> and <c>HardCommitted</c>.
/// Any other value is rejected.
/// </summary>
public sealed class ResourceBookingTypeJsonConverter : JsonConverter<ResourceBookingType>
{
    private const string Soft = "SoftTentative";
    private const string Hard = "HardCommitted";

    /// <summary>
    /// Gets the name the API uses for a booking type, in bodies and in query strings.
    /// </summary>
    /// <param name="value">The booking type.</param>
    /// <returns>The name.</returns>
    public static string ToWire(ResourceBookingType value) => value switch
    {
        ResourceBookingType.SoftTentative => Soft,
        ResourceBookingType.HardCommitted => Hard,
        _ => throw new JsonException("Unknown booking type: " + value + "."),
    };

    /// <inheritdoc/>
    public override ResourceBookingType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;

        return text switch
        {
            Soft => ResourceBookingType.SoftTentative,
            Hard => ResourceBookingType.HardCommitted,
            _ => throw new JsonException("Unknown booking type: " + (text ?? reader.TokenType.ToString()) + "."),
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, ResourceBookingType value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStringValue(ToWire(value));
    }
}
