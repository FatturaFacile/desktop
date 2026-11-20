// <copyright file="CopilotApiMapper.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using FatturaFacile.Application.Copilot;

namespace FatturaFacile.Infrastructure.Copilot;

/// <summary>
/// Maps the JSON resources of the Copilot API to the application models. Tolerant: missing fields become empty values.
/// </summary>
public static class CopilotApiMapper
{
    /// <summary>
    /// Maps an array of conversation resources.
    /// </summary>
    /// <param name="json">The array.</param>
    /// <returns>The conversations.</returns>
    public static IReadOnlyList<CopilotConversation> ToConversations(JsonElement json) => [.. Items(json).Select(ToConversation)];

    /// <summary>
    /// Maps a conversation resource (with optional messages).
    /// </summary>
    /// <param name="json">The resource.</param>
    /// <returns>The conversation.</returns>
    public static CopilotConversation ToConversation(JsonElement json) =>
        new(
            Text(json, "id") ?? string.Empty,
            Text(json, "title") ?? string.Empty,
            Text(json, "locale") ?? string.Empty,
            new CopilotContext(Text(json, "context_route"), Text(json, "context_entity_type"), Text(json, "context_entity_id")),
            Date(json, "created_at"),
            Date(json, "updated_at"),
            [.. Items(Member(json, "messages")).Select(ToMessage)]);

    /// <summary>
    /// Maps a message resource.
    /// </summary>
    /// <param name="json">The resource.</param>
    /// <returns>The message.</returns>
    public static CopilotMessage ToMessage(JsonElement json)
    {
        var visualization = Member(json, "visualization");
        var action = Member(json, "proposed_action");

        return new CopilotMessage(
            Text(json, "id") ?? string.Empty,
            Text(json, "conversation_id") ?? string.Empty,
            Text(json, "role") ?? "assistant",
            Text(json, "content_markdown") ?? string.Empty,
            [.. Items(Member(json, "citations")).Select(ToCitation)],
            visualization.ValueKind == JsonValueKind.Object ? ToVisualization(visualization) : null,
            action.ValueKind == JsonValueKind.Object ? ToProposedAction(action) : null,
            Text(json, "action_status"),
            Date(json, "created_at"));
    }

    /// <summary>
    /// Maps a citation.
    /// </summary>
    /// <param name="json">The citation.</param>
    /// <returns>The citation model.</returns>
    public static CopilotCitation ToCitation(JsonElement json) =>
        new(
            Text(json, "type") ?? "regulatory",
            Text(json, "label") ?? string.Empty,
            Text(json, "title"),
            Text(json, "section_ref"),
            Text(json, "jurisdiction"),
            Text(json, "entity_type"),
            Text(json, "entity_id"),
            Text(json, "route"));

    /// <summary>
    /// Maps a visualization payload.
    /// </summary>
    /// <param name="json">The payload.</param>
    /// <returns>The visualization.</returns>
    public static CopilotVisualization ToVisualization(JsonElement json)
    {
        List<IReadOnlyDictionary<string, string>> rows = [];

        foreach (var row in Items(Member(json, "rows")).Where(static r => r.ValueKind == JsonValueKind.Object))
        {
            rows.Add(Properties(row).ToDictionary(static p => p.Name, static p => Scalar(p.Value), StringComparer.Ordinal));
        }

        List<CopilotDataset> datasets = [];

        foreach (var set in Items(Member(json, "datasets")).Where(static s => s.ValueKind == JsonValueKind.Object))
        {
            datasets.Add(new CopilotDataset(Text(set, "name") ?? Text(set, "label") ?? string.Empty, [.. Items(Member(set, "data")).Select(ToDouble)]));
        }

        var value = Member(json, "value");

        return new CopilotVisualization(
            Text(json, "type") ?? "table",
            Text(json, "title") ?? string.Empty,
            Strings(json, "columns"),
            rows,
            Text(json, "x"),
            Strings(json, "labels"),
            datasets,
            Text(json, "metric"),
            IsMissing(value) ? null : Scalar(value));
    }

    /// <summary>
    /// Maps a proposed action. Arguments are flattened into dotted paths so they can be edited and sent back as overrides.
    /// </summary>
    /// <param name="json">The proposed action.</param>
    /// <returns>The action.</returns>
    public static CopilotProposedAction ToProposedAction(JsonElement json)
    {
        List<CopilotArgument> arguments = [];
        Flatten(Member(json, "arguments"), string.Empty, arguments);

        var details = Properties(Member(json, "details")).ToDictionary(static p => p.Name, static p => Scalar(p.Value), StringComparer.Ordinal);
        var result = Member(json, "result");

        return new CopilotProposedAction(
            Text(json, "tool") ?? string.Empty,
            Text(json, "module") ?? string.Empty,
            Text(json, "status") ?? "pending",
            Text(json, "summary") ?? string.Empty,
            arguments,
            details,
            Strings(json, "editable_fields"),
            IsMissing(result) ? null : Scalar(result),
            Text(json, "error"));
    }

    /// <summary>
    /// Maps a raw server-sent event to a stream event; unknown events yield null.
    /// </summary>
    /// <param name="raw">The raw event.</param>
    /// <returns>The event, or null.</returns>
    public static CopilotStreamEvent? ToStreamEvent(CopilotSseEvent raw)
    {
        ArgumentNullException.ThrowIfNull(raw);

        var data = ParseData(raw.Data);
        var isObject = data.ValueKind == JsonValueKind.Object;

        return raw.Name switch
        {
            "message.started" => new CopilotStreamEvent(CopilotStreamEventKind.MessageStarted),
            "tool.call" => new CopilotStreamEvent(CopilotStreamEventKind.ToolCall, Text(data, "name") ?? Text(data, "tool")),
            "tool.result" => new CopilotStreamEvent(CopilotStreamEventKind.ToolResult, Text(data, "name") ?? Text(data, "tool")),
            "visualization" when isObject => new CopilotStreamEvent(CopilotStreamEventKind.Visualization, Visualization: ToVisualization(Unwrap(data, "visualization"))),
            "action.proposed" when isObject => new CopilotStreamEvent(CopilotStreamEventKind.ActionProposed, ProposedAction: ToProposedAction(Unwrap(data, "proposed_action")), MessageId: Text(data, "message_id")),
            "token" => new CopilotStreamEvent(CopilotStreamEventKind.Token, isObject ? Text(data, "text") : raw.Data),
            "message.completed" when isObject => new CopilotStreamEvent(CopilotStreamEventKind.MessageCompleted, MessageId: Text(data, "message_id"), ConversationId: Text(data, "conversation_id")),
            "error" => new CopilotStreamEvent(CopilotStreamEventKind.Error, isObject ? Text(data, "message") : raw.Data),
            _ => null,
        };
    }

    private static JsonElement ParseData(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return default;
        }

        try
        {
            using var document = JsonDocument.Parse(text);
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static JsonElement Unwrap(JsonElement data, string wrapper)
    {
        var inner = Member(data, wrapper);
        return inner.ValueKind == JsonValueKind.Object ? inner : data;
    }

    private static void Flatten(JsonElement element, string path, List<CopilotArgument> into)
    {
        var kind = element.ValueKind;

        if (kind == JsonValueKind.Object)
        {
            foreach (var property in Properties(element))
            {
                Flatten(property.Value, path.Length == 0 ? property.Name : path + "." + property.Name, into);
            }
        }
        else if (kind == JsonValueKind.Array)
        {
            var index = 0;

            foreach (var item in Items(element))
            {
                var key = index.ToString(CultureInfo.InvariantCulture);
                Flatten(item, path.Length == 0 ? key : path + "." + key, into);
                index++;
            }
        }
        else if (kind == JsonValueKind.Number)
        {
            into.Add(new CopilotArgument(path, element.GetRawText(), CopilotArgumentKind.Number));
        }
        else if (kind is JsonValueKind.True or JsonValueKind.False)
        {
            into.Add(new CopilotArgument(path, kind == JsonValueKind.True ? "true" : "false", CopilotArgumentKind.Boolean));
        }
        else if (kind == JsonValueKind.String)
        {
            into.Add(new CopilotArgument(path, element.GetString() ?? string.Empty, CopilotArgumentKind.Text));
        }
    }

    private static List<JsonElement> Items(JsonElement array)
    {
        var length = array.ValueKind == JsonValueKind.Array ? array.GetArrayLength() : 0;
        List<JsonElement> items = [];

        for (var i = 0; i < length; i++)
        {
            items.Add(array[i]);
        }

        return items;
    }

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP004:Don't ignore created IDisposable", Justification = "The object enumerator is a struct whose Dispose is a no-op; foreach disposes it.")]
    private static List<JsonProperty> Properties(JsonElement obj)
    {
        List<JsonProperty> properties = [];

        if (obj.ValueKind == JsonValueKind.Object)
        {
            properties.AddRange(obj.EnumerateObject());
        }

        return properties;
    }

    private static bool IsMissing(JsonElement value) => value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null;

    private static JsonElement Member(JsonElement json, string name) =>
        json.ValueKind == JsonValueKind.Object && json.TryGetProperty(name, out var value) ? value : default;

    private static string? Text(JsonElement json, string name)
    {
        var value = Member(json, name);
        return IsMissing(value) ? null : Scalar(value);
    }

    private static string[] Strings(JsonElement json, string name) => [.. Items(Member(json, name)).Select(Scalar)];

    private static double ToDouble(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Number)
        {
            return value.GetDouble();
        }

        _ = double.TryParse(Scalar(value), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed);
        return parsed;
    }

    private static DateTimeOffset? Date(JsonElement json, string name) =>
        Text(json, name) is { } text && DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date) ? date : null;

    private static string Scalar(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? string.Empty,
        JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Object or JsonValueKind.Array or JsonValueKind.Number => value.GetRawText(),
        _ => value.GetRawText(),
    };
}
