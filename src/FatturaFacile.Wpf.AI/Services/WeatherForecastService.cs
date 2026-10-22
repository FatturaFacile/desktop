// <copyright file="WeatherForecastService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Model;

namespace FatturaFacile.Services;

/// <summary>
/// Generates simulated daily weather forecast collections for UI and Blazor demo components.
/// </summary>
public class WeatherForecastService
{
    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly",
        "Cool", "Mild", "Warm",
        "Balmy", "Hot", "Sweltering", "Scorching"
    };

    /// <summary>
    /// Generates a synthetic 20-day weather forecast starting from the specified date.
    /// </summary>
    /// <param name="startDate">The anchor date from which forecast days are sequentially calculated.</param>
    /// <returns>A collection of generated <see cref="WeatherForecast"/> entries.</returns>
    public Task<IReadOnlyCollection<WeatherForecast>> GetForecastAsync(
        DateOnly startDate)
    {
        var rng = DevExpress.Data.Utils.NonCryptographicRandom.Default;
        IReadOnlyCollection<WeatherForecast> forecasts = Enumerable.Range(1, 20).Select(index =>
            new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToArray();
        return Task.FromResult(forecasts);
    }
}
