// <copyright file="WeatherAgentTools.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Text.Json;

using FatturaFacile.Services;

using Microsoft.Extensions.AI;

namespace FatturaFacile.Agents.Weather;

/// <summary>
/// Tool functions for retrieving city weather forecasts and JSON payloads for AI assistants.
/// </summary>
public class WeatherAgentTools
{
    private readonly WeatherService weatherService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherAgentTools"/> class.
    /// </summary>
    /// <param name="weatherService">The weather data lookup service.</param>
    public WeatherAgentTools(
        WeatherService weatherService)
    {
        this.weatherService = weatherService;
    }

    /// <summary>
    /// Creates and returns an <see cref="AIFunction"/> tool that queries city weather data.
    /// </summary>
    /// <returns>A configured weather lookup <see cref="AIFunction"/>.</returns>
    public AIFunction GetWeatherTool()
        => AIFunctionFactory.Create(
            (Func<string, Task<string>>)GetWeather, "GetWeather",
            "Returns the current weather and 5-day forecast for a specified city as JSON.");

    [Description(
        "Gets current weather and 5-day forecast for a specified city. Returns a beautifully formatted weather widget with temperature, humidity, wind speed, and daily forecasts.")]
    private async Task<string> GetWeather(
        [Description("The name of the city to get weather for (e.g., 'London', 'New York', 'Tokyo')")]
        string cityName)
    {
        var weather = await weatherService.GetWeatherAsync(cityName).ConfigureAwait(false);

        if (weather == null)
        {
            return $"Sorry, I couldn't fetch weather data for '{cityName}'. Please check the city name and try again.";
        }

        return JsonSerializer.Serialize(weather, new JsonSerializerOptions { WriteIndented = true });
    }
}
