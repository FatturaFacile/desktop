// <copyright file="WeatherForecast.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Model;

/// <summary>
/// Daily weather forecast summary item.
/// </summary>
public class WeatherForecast
{
    /// <summary>
    /// Gets or sets the forecast date.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Gets or sets the temperature in degrees Celsius.
    /// </summary>
    public int TemperatureC { get; set; }

    /// <summary>
    /// Gets the temperature in degrees Fahrenheit.
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    /// <summary>
    /// Gets or sets the textual weather summary description.
    /// </summary>
    public string Summary { get; set; } = string.Empty;
}
