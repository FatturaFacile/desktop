// <copyright file="WeatherService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.Data.Utils;
using FatturaFacile.Wpf.AI.Model;

namespace FatturaFacile.Wpf.AI.Services;

/// <summary>
/// Simulates real-time weather retrieval and 5-day forecasts for cities.
/// </summary>
public class WeatherService
{

    /// <summary>
    /// Simulated weather condition descriptions.
    /// </summary>
    private static readonly string[] Descriptions = new[]
                                                    {
                                                        "clear sky", "few clouds", "scattered clouds", "broken clouds",
                                                        "shower rain", "rain", "thunderstorm", "snow", "mist",
                                                    };

    /// <summary>
    /// Simulated weather icon codes.
    /// </summary>
    private static readonly string[] Icons = new[]
                                             {
                                                 "01d", "02d", "03d", "04d", "09d", "10d", "11d", "13d", "50d",
                                             };

    /// <summary>
    /// Country codes assigned during weather simulation.
    /// </summary>
    private static readonly string[] Countries = new[]
                                                 {
                                                     "GB", "US", "JP", "FR", "DE", "AU", "CA", "IT", "ES", "BR",
                                                 };

    /// <summary>
    /// Simulates retrieving current weather metrics and a 5-day projection for a specified municipality.
    /// </summary>
    /// <param name="cityName">The requested city or municipality name.</param>
    /// <returns>A populated <see cref="WeatherWidgetData"/> model.</returns>
    public Task<WeatherWidgetData> GetWeatherAsync(string cityName)
    {
        var rng       = NonCryptographicRandom.System;
        var descIndex = rng.Next(maxValue: Descriptions.Length);
        var temp      = Math.Round(value: rng.NextDouble() * 40 - 5, digits: 1);
        var now       = DateTime.Now;

        var forecast = new List<DailyForecast>();

        for (var i = 1; i <= 5; i++)
        {
            var fi = rng.Next(maxValue: Descriptions.Length);
            var hi = Math.Round(value: temp + rng.NextDouble() * 8, digits: 1);
            var lo = Math.Round(value: temp - rng.NextDouble() * 8, digits: 1);

            forecast.Add(
                         item: new DailyForecast
                               {
                                   Date        = now.AddDays(value: i),
                                   MaxTemp     = hi,
                                   MinTemp     = lo,
                                   Description = Descriptions[fi],
                                   Icon        = Icons[fi],
                                   Humidity    = rng.Next(minValue: 30, maxValue: 95),
                               });
        }

        var result = new WeatherWidgetData
                     {
                         CityName        = cityName,
                         Country         = Countries[rng.Next(maxValue: Countries.Length)],
                         Temperature     = temp,
                         FeelsLike       = Math.Round(value: temp + rng.NextDouble() * 4 - 2, digits: 1),
                         Description     = Descriptions[descIndex],
                         Icon            = Icons[descIndex],
                         Humidity        = rng.Next(minValue: 30, maxValue: 95),
                         WindSpeed       = Math.Round(value: rng.NextDouble() * 15, digits: 1),
                         Pressure        = rng.Next(minValue: 990, maxValue: 1040),
                         Sunrise         = now.Date.AddHours(value: 5  + rng.NextDouble() * 2),
                         Sunset          = now.Date.AddHours(value: 18 + rng.NextDouble() * 2),
                         FiveDayForecast = forecast,
                     };

        return Task.FromResult(result: result);
    }

}
