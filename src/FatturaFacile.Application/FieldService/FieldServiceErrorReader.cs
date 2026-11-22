// <copyright file="FieldServiceErrorReader.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Reads the error answers of the web-api (Laravel validation `errors` objects and RFC 7807 problem details).
/// </summary>
public static class FieldServiceErrorReader
{
    private const string Separator = "; ";

    /// <summary>
    /// Reads the messages of an `errors` object by field.
    /// </summary>
    /// <param name="body">The body of the error answer.</param>
    /// <returns>The messages by field, empty when the body has no `errors` object or is not JSON.</returns>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> Blockers(string body)
    {
        ArgumentNullException.ThrowIfNull(body);

        try
        {
            using var document = JsonDocument.Parse(body);

            return ReadErrors(document.RootElement);
        }
        catch (JsonException)
        {
            return new Dictionary<string, IReadOnlyList<string>>();
        }
    }

    /// <summary>
    /// Describes an error answer in one line: the field messages, or the `message`/`detail`/`title` of the problem.
    /// </summary>
    /// <param name="body">The body of the error answer.</param>
    /// <returns>The description, never empty.</returns>
    public static string Describe(string body)
    {
        ArgumentNullException.ThrowIfNull(body);

        var blockers = Blockers(body);
        if (blockers.Count > 0)
        {
            return string.Join(Separator, blockers.Select(pair => pair.Key + ": " + string.Join(", ", pair.Value)));
        }

        return Problem(body) ?? body;
    }

    /// <summary>
    /// Describes one failed operation of a mobile-sync result.
    /// </summary>
    /// <param name="result">The result object of the operation.</param>
    /// <returns>The description, never empty.</returns>
    public static string Describe(JsonElement result)
    {
        var errors = ReadErrors(result);

        return errors.Count > 0
            ? string.Join(Separator, errors.Select(pair => pair.Key + ": " + string.Join(", ", pair.Value)))
            : FieldServiceJson.NullableText(result, "status") ?? "rejected";
    }

    private static Dictionary<string, IReadOnlyList<string>> ReadErrors(JsonElement root)
    {
        Dictionary<string, IReadOnlyList<string>> map = new(StringComparer.Ordinal);
        if (FieldServiceJson.Child(root, "errors") is not { ValueKind: JsonValueKind.Object } errors)
        {
            return map;
        }

        foreach (var field in errors.EnumerateObject())
        {
            map[field.Name] = field.Value.ValueKind == JsonValueKind.Array
                ? [.. field.Value.EnumerateArray().Select(item => item.ToString())]
                : [field.Value.ToString()];
        }

        return map;
    }

    private static string? Problem(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            return FieldServiceJson.NullableText(root, "detail") ?? FieldServiceJson.NullableText(root, "message") ?? FieldServiceJson.NullableText(root, "title");
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
