// <copyright file="AuditChain.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FatturaFacile.Application.Security;

/// <summary>
/// Hashing rules of the offline audit chain: canonical JSON, payload hash (SHA-256) and record hash (HMAC-SHA256).
/// </summary>
public static class AuditChain
{
    /// <summary>
    /// The previous hash of the first entry of a chain.
    /// </summary>
    public static readonly string GenesisHash = new('0', 64);

    /// <summary>
    /// Serializes a value as canonical JSON: object keys sorted ordinally, no insignificant whitespace.
    /// </summary>
    /// <param name="value">The value (a JSON node, JSON text, any serializable object, or null).</param>
    /// <returns>The canonical JSON text.</returns>
    public static string ToCanonicalJson(object? value)
    {
        var node = value switch
        {
            null => null,
            JsonNode existing => existing.DeepClone(),
            string json when TryParse(json, out var parsed) => parsed,
            _ => JsonSerializer.SerializeToNode(value),
        };

        StringBuilder builder = new();
        Write(builder, node);
        return builder.ToString();
    }

    /// <summary>
    /// Computes the lowercase hex SHA-256 of the canonical payload JSON.
    /// </summary>
    /// <param name="canonicalJson">The canonical JSON text.</param>
    /// <returns>The payload hash.</returns>
    public static string ComputePayloadHash(string canonicalJson)
    {
        ArgumentNullException.ThrowIfNull(canonicalJson);
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalJson)));
    }

    /// <summary>
    /// Builds the metadata string that is bound into the record hash.
    /// </summary>
    /// <param name="sequenceNumber">The sequence number.</param>
    /// <param name="occurredAtUtc">The occurrence instant (UTC).</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="entityId">The entity identifier.</param>
    /// <param name="action">The action.</param>
    /// <param name="deviceId">The device identifier.</param>
    /// <returns>The metadata string.</returns>
    public static string BuildMetadata(long sequenceNumber, DateTime occurredAtUtc, string entityType, string entityId, string action, string deviceId) =>
        string.Join(
            '\n',
            sequenceNumber.ToString(CultureInfo.InvariantCulture),
            FormatInstant(occurredAtUtc),
            entityType,
            entityId,
            action,
            deviceId);

    /// <summary>
    /// Computes the record hash: HMAC-SHA256(previous_hash || payload_hash || metadata) keyed with the device key.
    /// </summary>
    /// <param name="key">The device key.</param>
    /// <param name="previousHash">The previous record hash.</param>
    /// <param name="payloadHash">The payload hash.</param>
    /// <param name="metadata">The metadata string.</param>
    /// <returns>The lowercase hex record hash.</returns>
    public static string ComputeRecordHash(byte[] key, string previousHash, string payloadHash, string metadata)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(previousHash);
        ArgumentNullException.ThrowIfNull(payloadHash);
        ArgumentNullException.ThrowIfNull(metadata);

        var data = Encoding.UTF8.GetBytes($"{previousHash}\n{payloadHash}\n{metadata}");
        return Convert.ToHexStringLower(HMACSHA256.HashData(key, data));
    }

    /// <summary>
    /// Formats an instant the way it is signed and uploaded (ISO 8601, UTC, 7 fractional digits).
    /// </summary>
    /// <param name="utc">The instant.</param>
    /// <returns>The text.</returns>
    public static string FormatInstant(DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture);

    private static bool TryParse(string json, out JsonNode? node)
    {
        try
        {
            node = JsonNode.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            node = null;
            return false;
        }
    }

    private static void Write(StringBuilder builder, JsonNode? node)
    {
        switch (node)
        {
            case null:
                _ = builder.Append("null");
                break;
            case JsonObject obj:
                WriteObject(builder, obj);
                break;
            case JsonArray array:
                _ = builder.Append('[');
                for (var i = 0; i < array.Count; i++)
                {
                    if (i > 0)
                    {
                        _ = builder.Append(',');
                    }

                    Write(builder, array[i]);
                }

                _ = builder.Append(']');
                break;
            default:
                _ = builder.Append(node.ToJsonString());
                break;
        }
    }

    private static void WriteObject(StringBuilder builder, JsonObject obj)
    {
        _ = builder.Append('{');
        var first = true;
        foreach (var property in obj.OrderBy(static p => p.Key, StringComparer.Ordinal))
        {
            if (!first)
            {
                _ = builder.Append(',');
            }

            first = false;
            _ = builder.Append(JsonSerializer.Serialize(property.Key)).Append(':');
            Write(builder, property.Value);
        }

        _ = builder.Append('}');
    }
}
