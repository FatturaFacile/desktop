// <copyright file="WeatherAgentTools.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Text.Json;
using FatturaFacile.Wpf.AI.Services;
using Microsoft.Extensions.AI;

namespace FatturaFacile.Wpf.AI.Agents.Weather;

/// <summary>
/// Tool functions for retrieving city weather forecasts and JSON payloads for AI assistants.
/// </summary>
public class WeatherAgentTools
{

    /// <summary>
    /// The weather data lookup service.
    /// </summary>
    private readonly WeatherService weatherService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherAgentTools"/> class.
    /// </summary>
    /// <param name="weatherService">The weather data lookup service.</param>
    public WeatherAgentTools(WeatherService weatherService)
    {
        this.weatherService = weatherService;
    }

    /// <summary>
    /// Creates and returns an <see cref="AIFunction"/> tool that queries city weather data.
    /// </summary>
    /// <returns>A configured weather lookup <see cref="AIFunction"/>.</returns>
    public AIFunction GetWeatherTool()
        => AIFunctionFactory.Create(
                                    method: (Func<string, Task<string>>)GetWeather,
                                    name: "GetWeather",
                                    description: "Returns the current weather and 5-day forecast for a specified city as JSON.");

    /// <summary>
    /// Queries current weather and forecast for the requested city.
    /// </summary>
    /// <param name="cityName">The name of the target city.</param>
    /// <returns>A JSON string containing the structured weather report.</returns>
    [Description(
                    description: "Gets current weather and 5-day forecast for a specified city. Returns a beautifully formatted weather widget with temperature, humidity, wind speed, and daily forecasts.")]
    private async Task<string> GetWeather(
        [Description(description: "The name of the city to get weather for (e.g., 'London', 'New York', 'Tokyo')")] string cityName)
    {
        var weather = await weatherService.GetWeatherAsync(cityName: cityName).ConfigureAwait(continueOnCapturedContext: false);

        return weather == null
            ? $"Sorry, I couldn't fetch weather data for '{cityName}'. Please check the city name and try again."
            : JsonSerializer.Serialize(
                                        value: weather,
                                        options: new JsonSerializerOptions
                                                 {
                                                     WriteIndented = true,
                                                 });
    }

}
