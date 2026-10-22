// <copyright file="WeatherWidgetData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Wpf.AI.Model;

/// <summary>
/// Aggregates current ambient weather metrics and a multi-day forecast tailored for UI widget presentation.
/// </summary>
public class WeatherWidgetData
{

    /// <summary>Gets or sets the targeted locality or municipality name.</summary>
    public string CityName { get; set; } = string.Empty;

    /// <summary>Gets or sets the country name or ISO two-letter territory code.</summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>Gets or sets the current ambient air temperature.</summary>
    public double Temperature { get; set; }

    /// <summary>Gets or sets the apparent human-perceived temperature accounting for humidity and wind chill.</summary>
    public double FeelsLike { get; set; }

    /// <summary>Gets or sets the textual description of primary weather conditions.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the weather condition icon glyph identifier.</summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>Gets or sets the relative atmospheric humidity percentage (0–100%).</summary>
    public int Humidity { get; set; }

    /// <summary>Gets or sets the measured wind velocity in meters per second.</summary>
    public double WindSpeed { get; set; }

    /// <summary>Gets or sets the barometric atmospheric pressure measured in hectopascals (hPa).</summary>
    public int Pressure { get; set; }

    /// <summary>Gets or sets the local astronomical sunrise timestamp.</summary>
    public DateTime Sunrise { get; set; }

    /// <summary>Gets or sets the local astronomical sunset timestamp.</summary>
    public DateTime Sunset { get; set; }

    /// <summary>Gets or sets the ordered five-day weather forecast projections.</summary>
    public List<DailyForecast> FiveDayForecast { get; set; } = new List<DailyForecast>();

}

/// <summary>
/// Represents projected meteorological conditions and temperature extremes for a single calendar day.
/// </summary>
public class DailyForecast
{

    /// <summary>Gets or sets the forecast calendar date.</summary>
    public DateTime Date { get; set; }

    /// <summary>Gets or sets the projected daily maximum temperature.</summary>
    public double MaxTemp { get; set; }

    /// <summary>Gets or sets the projected daily minimum temperature.</summary>
    public double MinTemp { get; set; }

    /// <summary>Gets or sets the primary weather conditions summary for the day.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the representative weather symbol/icon identifier.</summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>Gets or sets the expected mean relative humidity percentage (0–100%).</summary>
    public int Humidity { get; set; }

}

/// <summary>
/// Root OpenWeatherMap weather API response payload describing current meteorological observations.
/// </summary>
public class WeatherData
{

    /// <summary>Gets or sets geographic coordinates of the observation site.</summary>
    [JsonPropertyName(name: "coord")]
    public Coordinates Coordinates { get; set; } = null!;

    /// <summary>Gets or sets the collection of observed weather conditions.</summary>
    [JsonPropertyName(name: "weather")]
    public List<WeatherInfo> Weather { get; set; } = new();

    /// <summary>Gets or sets thermodynamic and barometric atmospheric measurements.</summary>
    [JsonPropertyName(name: "main")]
    public MainWeatherData Main { get; set; } = null!;

    /// <summary>Gets or sets anemological observations including velocity and direction.</summary>
    [JsonPropertyName(name: "wind")]
    public WindData Wind { get; set; } = null!;

    /// <summary>Gets or sets cloud coverage metrics.</summary>
    [JsonPropertyName(name: "clouds")]
    public CloudsData Clouds { get; set; } = null!;

    /// <summary>Gets or sets the UTC Unix timestamp of the weather observation.</summary>
    [JsonPropertyName(name: "dt")]
    public long Timestamp { get; set; }

    /// <summary>Gets or sets astronomical solar schedule and country metadata.</summary>
    [JsonPropertyName(name: "sys")]
    public SysData Sys { get; set; } = null!;

    /// <summary>Gets or sets the UTC timezone offset in seconds.</summary>
    [JsonPropertyName(name: "timezone")]
    public int Timezone { get; set; }

    /// <summary>Gets or sets the recorded municipal or station name.</summary>
    [JsonPropertyName(name: "name")]
    public string CityName { get; set; } = string.Empty;

}

/// <summary>
/// Represents geographic coordinates specified by longitude and latitude coordinates.
/// </summary>
public class Coordinates
{

    /// <summary>Gets or sets the geographic longitude coordinate in decimal degrees.</summary>
    [JsonPropertyName(name: "lon")]
    public double Longitude { get; set; }

    /// <summary>Gets or sets the geographic latitude coordinate in decimal degrees.</summary>
    [JsonPropertyName(name: "lat")]
    public double Latitude { get; set; }

}

/// <summary>
/// Describes a categorized meteorological condition including standard classification codes and icon references.
/// </summary>
public class WeatherInfo
{

    /// <summary>Gets or sets the condition identification code mapped to weather phenomena categories.</summary>
    [JsonPropertyName(name: "id")]
    public int Id { get; set; }

    /// <summary>Gets or sets the high-level weather parameter group (e.g., "Rain", "Snow", "Clear").</summary>
    [JsonPropertyName(name: "main")]
    public string Main { get; set; } = string.Empty;

    /// <summary>Gets or sets the granular description of the observed weather phenomenon.</summary>
    [JsonPropertyName(name: "description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the standard OpenWeatherMap icon identifier corresponding to the condition.</summary>
    [JsonPropertyName(name: "icon")]
    public string Icon { get; set; } = string.Empty;

}

/// <summary>
/// Contains thermodynamic and barometric measurements including temperatures, pressure, and humidity.
/// </summary>
public class MainWeatherData
{

    /// <summary>Gets or sets the instantaneous ambient temperature.</summary>
    [JsonPropertyName(name: "temp")]
    public double Temperature { get; set; }

    /// <summary>Gets or sets the human-perceived temperature accounting for relative humidity and wind.</summary>
    [JsonPropertyName(name: "feels_like")]
    public double FeelsLike { get; set; }

    /// <summary>Gets or sets the minimum currently observed temperature across the localized weather area.</summary>
    [JsonPropertyName(name: "temp_min")]
    public double MinTemperature { get; set; }

    /// <summary>Gets or sets the maximum currently observed temperature across the localized weather area.</summary>
    [JsonPropertyName(name: "temp_max")]
    public double MaxTemperature { get; set; }

    /// <summary>Gets or sets sea-level or station barometric atmospheric pressure in hectopascals (hPa).</summary>
    [JsonPropertyName(name: "pressure")]
    public int Pressure { get; set; }

    /// <summary>Gets or sets relative atmospheric humidity percentage (0–100%).</summary>
    [JsonPropertyName(name: "humidity")]
    public int Humidity { get; set; }

}

/// <summary>
/// Captures anemological measurements regarding wind speed and meteorological heading.
/// </summary>
public class WindData
{

    /// <summary>Gets or sets wind velocity measured in meters per second.</summary>
    [JsonPropertyName(name: "speed")]
    public double Speed { get; set; }

    /// <summary>Gets or sets wind direction expressed in meteorological azimuth degrees (0–360°).</summary>
    [JsonPropertyName(name: "deg")]
    public int Degree { get; set; }

}

/// <summary>
/// Measures percentage of sky obscured by clouds.
/// </summary>
public class CloudsData
{

    /// <summary>Gets or sets the cloudiness fraction expressed as a percentage (0–100%).</summary>
    [JsonPropertyName(name: "all")]
    public int All { get; set; }

}

/// <summary>
/// Contains geopolitical identifier and astronomical solar transition timestamps.
/// </summary>
public class SysData
{

    /// <summary>Gets or sets the ISO 3166-1 alpha-2 country code.</summary>
    [JsonPropertyName(name: "country")]
    public string Country { get; set; } = string.Empty;

    /// <summary>Gets or sets the sunrise epoch timestamp in UTC seconds.</summary>
    [JsonPropertyName(name: "sunrise")]
    public long Sunrise { get; set; }

    /// <summary>Gets or sets the sunset epoch timestamp in UTC seconds.</summary>
    [JsonPropertyName(name: "sunset")]
    public long Sunset { get; set; }

}

/// <summary>
/// Root OpenWeatherMap response object containing periodic multi-day forecast intervals and location details.
/// </summary>
public class ForecastData
{

    /// <summary>Gets or sets the list of forecasted intervals across the projection window.</summary>
    [JsonPropertyName(name: "list")]
    public List<ForecastItem> List { get; set; } = new();

    /// <summary>Gets or sets geographic and astronomical metadata for the target city.</summary>
    [JsonPropertyName(name: "city")]
    public CityData City { get; set; } = null!;

}

/// <summary>
/// A time-stamped weather forecast snapshot within a multi-day timeline.
/// </summary>
public class ForecastItem
{

    /// <summary>Gets or sets the projection epoch timestamp in UTC seconds.</summary>
    [JsonPropertyName(name: "dt")]
    public long Timestamp { get; set; }

    /// <summary>Gets or sets core thermodynamic metrics forecasted for this time point.</summary>
    [JsonPropertyName(name: "main")]
    public MainWeatherData Main { get; set; } = null!;

    /// <summary>Gets or sets forecasted weather condition descriptions.</summary>
    [JsonPropertyName(name: "weather")]
    public List<WeatherInfo> Weather { get; set; } = new();

    /// <summary>Gets or sets projected cloud coverage metrics.</summary>
    [JsonPropertyName(name: "clouds")]
    public CloudsData Clouds { get; set; } = null!;

    /// <summary>Gets or sets projected wind conditions.</summary>
    [JsonPropertyName(name: "wind")]
    public WindData Wind { get; set; } = null!;

    /// <summary>Gets or sets the formatted UTC date-time string (e.g., "yyyy-MM-dd HH:mm:ss").</summary>
    [JsonPropertyName(name: "dt_txt")]
    public string DateTimeText { get; set; } = string.Empty;

}

/// <summary>
/// Municipal and astronomical metadata for a forecasted location.
/// </summary>
public class CityData
{

    /// <summary>Gets or sets the official city or territory name.</summary>
    [JsonPropertyName(name: "name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the ISO country code.</summary>
    [JsonPropertyName(name: "country")]
    public string Country { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timezone offset in seconds.</summary>
    [JsonPropertyName(name: "timezone")]
    public int Timezone { get; set; }

    /// <summary>Gets or sets the local sunrise epoch timestamp in UTC seconds.</summary>
    [JsonPropertyName(name: "sunrise")]
    public long Sunrise { get; set; }

    /// <summary>Gets or sets the local sunset epoch timestamp in UTC seconds.</summary>
    [JsonPropertyName(name: "sunset")]
    public long Sunset { get; set; }

}
