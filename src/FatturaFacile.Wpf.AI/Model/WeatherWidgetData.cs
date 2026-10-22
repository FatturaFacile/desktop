// <copyright file="WeatherWidgetData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FatturaFacile.Models;

/// <summary>
/// Data model for rendering an interactive weather widget and 5-day forecast.
/// </summary>
public class WeatherWidgetData
{
    /// <summary>Gets or sets the city name.</summary>
    public string CityName { get; set; } = string.Empty;
    /// <summary>Gets or sets the country name or code.</summary>
    public string Country { get; set; } = string.Empty;
    /// <summary>Gets or sets the temperature value.</summary>
    public double Temperature { get; set; }
    /// <summary>Gets or sets the feels-like temperature value.</summary>
    public double FeelsLike { get; set; }
    /// <summary>Gets or sets the weather condition description.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Gets or sets the weather icon code.</summary>
    public string Icon { get; set; } = string.Empty;
    /// <summary>Gets or sets the humidity percentage.</summary>
    public int Humidity { get; set; }
    /// <summary>Gets or sets the wind speed in m/s or km/h.</summary>
    public double WindSpeed { get; set; }
    /// <summary>Gets or sets atmospheric pressure in hPa.</summary>
    public int Pressure { get; set; }
    /// <summary>Gets or sets sunrise local time.</summary>
    public DateTime Sunrise { get; set; }
    /// <summary>Gets or sets sunset local time.</summary>
    public DateTime Sunset { get; set; }
    /// <summary>Gets or sets the 5-day weather forecast collection.</summary>
    public List<DailyForecast> FiveDayForecast { get; set; } = new List<DailyForecast>();
}

/// <summary>
/// Weather forecast summary for a single day.
/// </summary>
public class DailyForecast
{
    /// <summary>Gets or sets the forecast date.</summary>
    public DateTime Date { get; set; }
    /// <summary>Gets or sets maximum expected temperature.</summary>
    public double MaxTemp { get; set; }
    /// <summary>Gets or sets minimum expected temperature.</summary>
    public double MinTemp { get; set; }
    /// <summary>Gets or sets weather condition description.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Gets or sets weather icon identifier.</summary>
    public string Icon { get; set; } = string.Empty;
    /// <summary>Gets or sets humidity percentage.</summary>
    public int Humidity { get; set; }
}

/// <summary>
/// Root OpenWeatherMap weather API JSON payload.
/// </summary>
public class WeatherData
{
    /// <summary>Gets or sets geographic coordinates.</summary>
    [JsonPropertyName("coord")] public Coordinates Coordinates { get; set; } = null!;

    /// <summary>Gets or sets weather condition descriptions.</summary>
    [JsonPropertyName("weather")] public List<WeatherInfo> Weather { get; set; } = new();

    /// <summary>Gets or sets main temperature metrics.</summary>
    [JsonPropertyName("main")] public MainWeatherData Main { get; set; } = null!;

    /// <summary>Gets or sets wind metrics.</summary>
    [JsonPropertyName("wind")] public WindData Wind { get; set; } = null!;

    /// <summary>Gets or sets cloudiness data.</summary>
    [JsonPropertyName("clouds")] public CloudsData Clouds { get; set; } = null!;

    /// <summary>Gets or sets timestamp.</summary>
    [JsonPropertyName("dt")] public long Timestamp { get; set; }

    /// <summary>Gets or sets system metrics.</summary>
    [JsonPropertyName("sys")] public SysData Sys { get; set; } = null!;

    /// <summary>Gets or sets timezone offset in seconds.</summary>
    [JsonPropertyName("timezone")] public int Timezone { get; set; }

    /// <summary>Gets or sets city name.</summary>
    [JsonPropertyName("name")] public string CityName { get; set; } = string.Empty;
}

/// <summary>
/// Geographical coordinates (longitude and latitude).
/// </summary>
public class Coordinates
{
    /// <summary>Gets or sets longitude.</summary>
    [JsonPropertyName("lon")] public double Longitude { get; set; }

    /// <summary>Gets or sets latitude.</summary>
    [JsonPropertyName("lat")] public double Latitude { get; set; }
}

/// <summary>
/// Weather condition details.
/// </summary>
public class WeatherInfo
{
    /// <summary>Gets or sets weather condition ID.</summary>
    [JsonPropertyName("id")] public int Id { get; set; }

    /// <summary>Gets or sets weather group parameters.</summary>
    [JsonPropertyName("main")] public string Main { get; set; } = string.Empty;

    /// <summary>Gets or sets weather condition within the group.</summary>
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets weather icon id.</summary>
    [JsonPropertyName("icon")] public string Icon { get; set; } = string.Empty;
}

/// <summary>
/// Main temperature and atmospheric metrics.
/// </summary>
public class MainWeatherData
{
    /// <summary>Gets or sets temperature.</summary>
    [JsonPropertyName("temp")] public double Temperature { get; set; }

    /// <summary>Gets or sets feels like temperature.</summary>
    [JsonPropertyName("feels_like")] public double FeelsLike { get; set; }

    /// <summary>Gets or sets minimum current temperature.</summary>
    [JsonPropertyName("temp_min")] public double MinTemperature { get; set; }

    /// <summary>Gets or sets maximum current temperature.</summary>
    [JsonPropertyName("temp_max")] public double MaxTemperature { get; set; }

    /// <summary>Gets or sets atmospheric pressure.</summary>
    [JsonPropertyName("pressure")] public int Pressure { get; set; }

    /// <summary>Gets or sets humidity.</summary>
    [JsonPropertyName("humidity")] public int Humidity { get; set; }
}

/// <summary>
/// Wind condition metrics.
/// </summary>
public class WindData
{
    /// <summary>Gets or sets wind speed.</summary>
    [JsonPropertyName("speed")] public double Speed { get; set; }

    /// <summary>Gets or sets wind direction in degrees.</summary>
    [JsonPropertyName("deg")] public int Degree { get; set; }
}

/// <summary>
/// Cloudiness information.
/// </summary>
public class CloudsData
{
    /// <summary>Gets or sets cloudiness percentage.</summary>
    [JsonPropertyName("all")] public int All { get; set; }
}

/// <summary>
/// Country and sun schedule metrics.
/// </summary>
public class SysData
{
    /// <summary>Gets or sets country code.</summary>
    [JsonPropertyName("country")] public string Country { get; set; } = string.Empty;

    /// <summary>Gets or sets sunrise time.</summary>
    [JsonPropertyName("sunrise")] public long Sunrise { get; set; }

    /// <summary>Gets or sets sunset time.</summary>
    [JsonPropertyName("sunset")] public long Sunset { get; set; }
}

/// <summary>
/// Five-day forecast list container.
/// </summary>
public class ForecastData
{
    /// <summary>Gets or sets forecast items list.</summary>
    [JsonPropertyName("list")] public List<ForecastItem> List { get; set; } = new();

    /// <summary>Gets or sets city information.</summary>
    [JsonPropertyName("city")] public CityData City { get; set; } = null!;
}

/// <summary>
/// Individual forecast snapshot item.
/// </summary>
public class ForecastItem
{
    /// <summary>Gets or sets snapshot timestamp.</summary>
    [JsonPropertyName("dt")] public long Timestamp { get; set; }

    /// <summary>Gets or sets main metrics.</summary>
    [JsonPropertyName("main")] public MainWeatherData Main { get; set; } = null!;

    /// <summary>Gets or sets weather condition list.</summary>
    [JsonPropertyName("weather")] public List<WeatherInfo> Weather { get; set; } = new();

    /// <summary>Gets or sets cloudiness information.</summary>
    [JsonPropertyName("clouds")] public CloudsData Clouds { get; set; } = null!;

    /// <summary>Gets or sets wind metrics.</summary>
    [JsonPropertyName("wind")] public WindData Wind { get; set; } = null!;

    /// <summary>Gets or sets formatted date time text.</summary>
    [JsonPropertyName("dt_txt")] public string DateTimeText { get; set; } = string.Empty;
}

/// <summary>
/// Forecast city metadata.
/// </summary>
public class CityData
{
    /// <summary>Gets or sets city name.</summary>
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets country code.</summary>
    [JsonPropertyName("country")] public string Country { get; set; } = string.Empty;

    /// <summary>Gets or sets timezone offset.</summary>
    [JsonPropertyName("timezone")] public int Timezone { get; set; }

    /// <summary>Gets or sets sunrise timestamp.</summary>
    [JsonPropertyName("sunrise")] public long Sunrise { get; set; }

    /// <summary>Gets or sets sunset timestamp.</summary>
    [JsonPropertyName("sunset")] public long Sunset { get; set; }
}
