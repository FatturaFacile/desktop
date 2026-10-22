using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIDemo.Models;
using DevExpress.Data.Utils;

namespace AIDemo.Services {
    public class WeatherService {
        static readonly string[] Descriptions = new[] { "clear sky", "few clouds", "scattered clouds", "broken clouds", "shower rain", "rain", "thunderstorm", "snow", "mist" };
        static readonly string[] Icons = new[] { "01d", "02d", "03d", "04d", "09d", "10d", "11d", "13d", "50d" };
        static readonly string[] Countries = new[] { "GB", "US", "JP", "FR", "DE", "AU", "CA", "IT", "ES", "BR" };

        public Task<WeatherWidgetData> GetWeatherAsync(string cityName) {
            var rng = NonCryptographicRandom.System;
            int descIndex = rng.Next(Descriptions.Length);
            double temp = Math.Round(rng.NextDouble() * 40 - 5, 1);
            var now = DateTime.Now;

            var forecast = new List<DailyForecast>();
            for(int i = 1; i <= 5; i++) {
                int fi = rng.Next(Descriptions.Length);
                double hi = Math.Round(temp + rng.NextDouble() * 8, 1);
                double lo = Math.Round(temp - rng.NextDouble() * 8, 1);
                forecast.Add(new DailyForecast {
                    Date = now.AddDays(i),
                    MaxTemp = hi,
                    MinTemp = lo,
                    Description = Descriptions[fi],
                    Icon = Icons[fi],
                    Humidity = rng.Next(30, 95)
                });
            }

            var result = new WeatherWidgetData {
                CityName = cityName,
                Country = Countries[rng.Next(Countries.Length)],
                Temperature = temp,
                FeelsLike = Math.Round(temp + rng.NextDouble() * 4 - 2, 1),
                Description = Descriptions[descIndex],
                Icon = Icons[descIndex],
                Humidity = rng.Next(30, 95),
                WindSpeed = Math.Round(rng.NextDouble() * 15, 1),
                Pressure = rng.Next(990, 1040),
                Sunrise = now.Date.AddHours(5 + rng.NextDouble() * 2),
                Sunset = now.Date.AddHours(18 + rng.NextDouble() * 2),
                FiveDayForecast = forecast
            };

            return Task.FromResult(result);
        }
    }
}
