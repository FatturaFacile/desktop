using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FatturaFacile.Model;

namespace FatturaFacile.Infrastructure;

public class WeatherForecastService
{
    static readonly string[] Summaries = {
        "Freezing", "Bracing", "Chilly",
        "Cool", "Mild", "Warm",
        "Balmy", "Hot", "Sweltering", "Scorching"
    };
    public Task<IReadOnlyCollection<WeatherForecast>> GetForecastAsync(DateOnly startDate)
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