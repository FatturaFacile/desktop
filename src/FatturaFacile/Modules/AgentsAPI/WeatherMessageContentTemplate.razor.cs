using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using DevExpress.AIIntegration.Blazor.Chat;
using AIDemo.Models;
using Markdig;

namespace AIDemo {
    public partial class WeatherMessageContentTemplate : ComponentBase {
        [Parameter] public BlazorChatMessage Message { get; set; }

        static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

        MarkupString ToHtml(string markdown) {
            string html = Markdown.ToHtml(markdown);
            return new MarkupString(html);
        }

        WeatherWidgetData GetWeatherFromFunctionCalls(List<AIChatFunctionCall> functionCalls) {
            if(functionCalls == null || functionCalls.Count == 0)
                return null;
            var weatherCall = functionCalls.Find(
                fc => fc.Request != null && fc.Request.Name == "GetWeather" && fc.Result != null && fc.Result.Result != null);
            if(weatherCall == null)
                return null;
            try {
                var json = weatherCall.Result.Result.ToString();
                if(string.IsNullOrWhiteSpace(json))
                    return null;
                var data = JsonSerializer.Deserialize<WeatherWidgetData>(json, JsonOptions);
                return data != null && data.CityName != null ? data : null;
            } catch {
                return null;
            }
        }
    }
}
