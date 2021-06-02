using BackEnd.Data;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BackEnd.Controllers
{
    [Route("api/weather")]
    [ApiController]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IWeatherForecastService _weatherForecastService;
        private readonly string[] cities = new string[]
        {
            "Warszawa",
            "Łódź",
            "Wrocław",
            "Szczecin",
            "Rzeszów",
            "Kraków",
            "Gdańsk",
            "Suwałki"
        };

        public WeatherForecastController(IWeatherForecastService weatherForecastService)
        {
            _weatherForecastService = weatherForecastService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<WeatherForecast>> GetAllWeatherForecasts()
        {
            IEnumerable<WeatherForecast> weatherForecasts = _weatherForecastService.GetAllWeatherForecasts();
            if (weatherForecasts.Count() <= 0) return NoContent();
            return Ok(weatherForecasts);
        }

        [HttpGet("{city}")]
        public ActionResult<IEnumerable<WeatherForecast>> GetAllWeatherForecasts(string city)
        {
            if (!cities.Contains(city)) return NotFound();
            IEnumerable<WeatherForecast> weatherForecasts = _weatherForecastService.GetWeatherForecastsByCity(city);
            if (weatherForecasts.Count() <= 0) return NoContent();
            return Ok(weatherForecasts);
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<WeatherForecast>>> AddAllWeatherForecasts()
        {
            IEnumerable<WeatherForecast> weatherForecasts = await _weatherForecastService.AddAllWeatherForecasts();
            if (weatherForecasts.Count() <= 0) return NotFound();
            return Ok(weatherForecasts);
        }

        private bool AlreadyCheckedDay(IEnumerable<DateTime> days, DateTime day)
        {
            foreach (DateTime tempDay in days)
            {
                if (tempDay.Date == day.Date) return true;
            }
            return false;
        }

        [ApiKeyAuth]
        [HttpPost("dbsize")]
        public ActionResult GetDays()
        {
            IEnumerable<DateTime> days = new List<DateTime>();
            IEnumerable<WeatherForecast> weatherForecasts = _weatherForecastService.GetAllWeatherForecasts();

            foreach (WeatherForecast weatherForecast in weatherForecasts)
            {
                if (!AlreadyCheckedDay(days, weatherForecast.Date))
                {
                    days = days.Append(weatherForecast.Date);
                }
            }

            int numberOfDays = days.Count();
            return Ok(new { numberOfDays });
        }

        private bool CorrectParameters(string p)
        {
            foreach (PropertyInfo property in typeof(WeatherForecast).GetProperties())
            {
                if (String.Equals(p, property.Name, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private bool CorrectParameters(string c, string p)
        {
            if (!cities.Contains(c)) return false;
            foreach (PropertyInfo property in typeof(WeatherForecast).GetProperties())
            {
                if (String.Equals(p, property.Name, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private WeatherForecast GetClosestForecast(IEnumerable<WeatherForecast> weatherForecasts)
        {
            WeatherForecast weatherForecast = weatherForecasts.First();
            foreach (WeatherForecast tempWeatherForecast in weatherForecasts)
            {
                if (DateTime.Compare(tempWeatherForecast.Date, weatherForecast.Date) < 0) weatherForecast = tempWeatherForecast;
            }
            return weatherForecast;
        }

        private float GetAverageValueForCity(string c, string p, int d)
        {
            IEnumerable<WeatherForecast> weatherForecasts = _weatherForecastService.GetWeatherForecastsByCity(c);
            IEnumerable<DateTime> days = new List<DateTime>();

            float averageValue = 0.0f;
            int numberOfValues = 0;

            while (weatherForecasts.Count() > 0)
            {
                WeatherForecast weatherForecast = GetClosestForecast(weatherForecasts);
                weatherForecasts = weatherForecasts.Where(tempWeatherForecast => tempWeatherForecast.Id != weatherForecast.Id).ToList();
                if (!AlreadyCheckedDay(days, weatherForecast.Date)) days = days.Append(weatherForecast.Date);
                if (days.Count() > d) break;

                ++numberOfValues;
                averageValue += (float)weatherForecast.GetType().GetProperty(p).GetValue(weatherForecast, null);
            }

            averageValue /= numberOfValues;
            return averageValue;
        }

        [ApiKeyAuth]
        [HttpPost("average")]
        public ActionResult GetAverageForCity(string c, string p, int d)
        {
            if (String.IsNullOrEmpty(c) || String.IsNullOrEmpty(p) || d <= 0) return NotFound();
            if (!CorrectParameters(c, p)) return NotFound();

            float averageValue = GetAverageValueForCity(c, p, d);

            return Ok(new { averageValue });
        }

        [ApiKeyAuth]
        [HttpPost("poland")]
        public ActionResult GetAverageForPoland(string p, int d)
        {
            if (String.IsNullOrEmpty(p) || d <= 0) return NotFound();
            if (!CorrectParameters(p)) return NotFound();

            float averageValue = 0.0f;
            foreach (string city in cities)
            {
                averageValue += GetAverageValueForCity(city, p, d);
            }

            averageValue /= cities.Count();
            return Ok(new { averageValue });
        }
    }
}
