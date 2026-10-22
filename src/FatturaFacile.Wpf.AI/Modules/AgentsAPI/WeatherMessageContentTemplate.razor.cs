// <copyright file="WeatherMessageContentTemplate.razor.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;

using Markdig;

using Microsoft.AspNetCore.Components;

using DevExpress.AIIntegration.Blazor.Chat;
using FatturaFacile.Wpf.AI.Model;

namespace FatturaFacile.Modules.AgentsAPI;

/// <summary>
/// Custom Blazor chat message template rendering embedded interactive weather widgets whenever tool calls return weather data.
/// </summary>
public partial class WeatherMessageContentTemplate : ComponentBase
{

    /// <summary>
    /// Gets or sets the chat message model being rendered within the UI bubble.
    /// </summary>
    [Parameter]
    public BlazorChatMessage Message { get; set; } = null!;

    /// <summary>
    /// JSON serialization options used to deserialize weather payloads.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
                                                                {
                                                                    PropertyNameCaseInsensitive = true,
                                                                };

    /// <summary>
    /// Converts raw markdown into HTML markup.
    /// </summary>
    /// <param name="markdown">The markdown text to convert.</param>
    /// <returns>A <see cref="MarkupString"/> representing the rendered HTML.</returns>
    private MarkupString ToHtml(string markdown)
    {
        var html = Markdown.ToHtml(markdown: markdown);

        return new MarkupString(value: html);
    }

    /// <summary>
    /// Extracts weather widget data from tool function call results.
    /// </summary>
    /// <param name="functionCalls">The list of function calls performed by the agent.</param>
    /// <returns>A deserialized <see cref="WeatherWidgetData"/> instance, or <c>null</c> if not found.</returns>
    private WeatherWidgetData? GetWeatherFromFunctionCalls(List<AIChatFunctionCall> functionCalls)
    {
        if (functionCalls == null || functionCalls.Count == 0)
        {
            return null;
        }

        var weatherCall = functionCalls.Find(match: fc => fc.Request != null && fc.Request.Name  == "GetWeather" &&
                                                          fc.Result  != null && fc.Result.Result != null);

        if (weatherCall == null)
        {
            return null;
        }

        try
        {
            var json = weatherCall.Result.Result.ToString();

            if (string.IsNullOrWhiteSpace(value: json))
            {
                return null;
            }

            var data = JsonSerializer.Deserialize<WeatherWidgetData>(json: json, options: JsonOptions);

            return data != null && data.CityName != null ? data : null;
        }
        catch
        {
            return null;
        }
    }

}
