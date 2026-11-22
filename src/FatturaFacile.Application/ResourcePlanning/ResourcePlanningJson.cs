// <copyright file="ResourcePlanningJson.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The JSON conventions of the resource planning API: snake case members, <c>Y-m-d</c> dates, enums as named by the contract, numbers that may arrive as strings.
/// </summary>
public static class ResourcePlanningJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Reads a document of the API.
    /// </summary>
    /// <typeparam name="T">The expected type.</typeparam>
    /// <param name="json">The JSON of the data member.</param>
    /// <returns>The value.</returns>
    public static T Read<T>(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return JsonSerializer.Deserialize<T>(json, Options) ?? throw new JsonException("The resource planning answer is empty.");
    }

    /// <summary>
    /// Writes a request body; members that are null are left out.
    /// </summary>
    /// <typeparam name="T">The type of the body.</typeparam>
    /// <param name="value">The body.</param>
    /// <returns>The JSON.</returns>
    public static string Write<T>(T value) => JsonSerializer.Serialize(value, Options);

    /// <summary>
    /// Gets the value of the data member of an answer.
    /// </summary>
    /// <param name="body">The body of the answer, with its success envelope.</param>
    /// <returns>The raw JSON of the data member.</returns>
    public static string ReadData(string body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        using var document = JsonDocument.Parse(body);

        return document.RootElement.ValueKind == JsonValueKind.Object && document.RootElement.TryGetProperty("data", out var data)
            ? data.GetRawText()
            : throw new JsonException("The resource planning answer has no data.");
    }

    /// <summary>
    /// Reads the violations of a refused request: the <c>invalid_params</c> of the problem document.
    /// </summary>
    /// <param name="body">The body of the answer; anything that is not a problem document gives no issues.</param>
    /// <returns>The issues, possibly none.</returns>
    public static IReadOnlyList<PlanningIssue> ReadIssues(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(body);

            return document.RootElement.ValueKind == JsonValueKind.Object && document.RootElement.TryGetProperty("invalid_params", out var issues) && issues.ValueKind == JsonValueKind.Array
                ? Read<List<PlanningIssue>>(issues.GetRawText())
                : [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    /// <summary>
    /// Formats a date as the API expects it.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The date as <c>yyyy-MM-dd</c>.</returns>
    public static string Day(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets the name the API uses for an enum value in a query string.
    /// </summary>
    /// <typeparam name="T">The enum.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>The lower snake case name.</returns>
    public static string Wire<T>(T value)
        where T : struct, Enum =>
        JsonNamingPolicy.SnakeCaseLower.ConvertName(value.ToString());
}
