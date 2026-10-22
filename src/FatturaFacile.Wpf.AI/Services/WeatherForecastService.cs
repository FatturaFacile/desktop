// <copyright file="WeatherForecastService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Wpf.AI.Model;

namespace FatturaFacile.Wpf.AI.Services;

/// <summary>
/// Generates simulated daily weather forecast collections for UI and Blazor demo components.
/// </summary>
public class WeatherForecastService
{

    /// <summary>
    /// Sample weather descriptive summaries used during simulation.
    /// </summary>
    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    /// <summary>
    /// Generates a synthetic 20-day weather forecast starting from the specified date.
    /// </summary>
    /// <param name="startDate">The anchor date from which forecast days are sequentially calculated.</param>
    /// <returns>A collection of generated <see cref="WeatherForecast"/> entries.</returns>
    public Task<IReadOnlyCollection<WeatherForecast>> GetForecastAsync(DateOnly startDate)
    {
        var rng = DevExpress.Data.Utils.NonCryptographicRandom.Default;

        IReadOnlyCollection<WeatherForecast> forecasts = Enumerable.Range(start: 1, count: 20)
                                                                   .Select(selector: index => new WeatherForecast
                                                                                              {
                                                                                                  Date = startDate.AddDays(value: index),
                                                                                                  TemperatureC =
                                                                                                      rng.Next(minValue: -20, maxValue: 55),
                                                                                                  Summary = Summaries[rng.Next(
                                                                                                                               maxValue: Summaries
                                                                                                                                  .Length)]
                                                                                              })
                                                                   .ToArray();

        return Task.FromResult(result: forecasts);
    }

}
