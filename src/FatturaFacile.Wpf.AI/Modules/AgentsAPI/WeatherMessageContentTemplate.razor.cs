// <copyright file="WeatherMessageContentTemplate.razor.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using Markdig;
using Microsoft.AspNetCore.Components;
using DevExpress.AIIntegration.Blazor.Chat;

using FatturaFacile.Models;

namespace FatturaFacile.Modules.AgentsAPI;

/// <summary>
/// Custom Blazor chat message template rendering embedded interactive weather widgets whenever tool calls return weather data.
/// </summary>
public partial class WeatherMessageContentTemplate : ComponentBase
{
    /// <summary>
    /// Gets or sets the chat message model being rendered within the UI bubble.
    /// </summary>
    [Parameter] public BlazorChatMessage Message { get; set; } = null!;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
        { PropertyNameCaseInsensitive = true };

    private MarkupString ToHtml(
        string markdown)
    {
        var html = Markdown.ToHtml(markdown);
        return new MarkupString(html);
    }

    private WeatherWidgetData? GetWeatherFromFunctionCalls(
        List<AIChatFunctionCall> functionCalls)
    {
        if (functionCalls == null || functionCalls.Count == 0)
        {
            return null;
        }

        var weatherCall = functionCalls.Find(fc =>
            fc.Request != null && fc.Request.Name == "GetWeather" && fc.Result != null && fc.Result.Result != null);
        if (weatherCall == null)
        {
            return null;
        }

        try
        {
            var json = weatherCall.Result.Result.ToString();
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var data = JsonSerializer.Deserialize<WeatherWidgetData>(json, JsonOptions);
            return data != null && data.CityName != null ? data : null;
        }
        catch
        {
            return null;
        }
    }
}
