// <copyright file="WeatherForecast.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Model;

/// <summary>
/// Represents a single-day weather forecast projection with temperature and conditions.
/// </summary>
public class WeatherForecast
{
    /// <summary>
    /// Gets or sets the calendar date of the forecasted weather conditions.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Gets or sets the projected temperature measured in degrees Celsius (°C).
    /// </summary>
    public int TemperatureC { get; set; }

    /// <summary>
    /// Gets the projected temperature converted to degrees Fahrenheit (°F).
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    /// <summary>
    /// Gets or sets the human-readable summary of the weather conditions (e.g., "Warm", "Rainy").
    /// </summary>
    public string Summary { get; set; } = string.Empty;
}
