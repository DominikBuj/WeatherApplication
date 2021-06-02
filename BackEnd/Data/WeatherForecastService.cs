using BackEnd.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BackEnd.Data
{
    public class WeatherForecastService : IWeatherForecastService
    {
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
        private readonly ApplicationDbContext _context;
        private readonly IOpenWeatherService _openWeatherService;

        public WeatherForecastService(ApplicationDbContext context, IOpenWeatherService openWeatherService)
        {
            _context = context;
            _openWeatherService = openWeatherService;
        }

        public IEnumerable<WeatherForecast> GetWeatherForecastsByCity(string city)
        {
            return _context.WeatherForecasts.Where(weatherForecast => weatherForecast.City == city).ToList();
        }

        public IEnumerable<WeatherForecast> GetAllWeatherForecasts()
        {
            return _context.WeatherForecasts.ToList();
        }

        public async Task<WeatherForecast> AddWeatherForecastByCity(string city)
        {
            WeatherForecast weatherForecast = await _openWeatherService.GetWeatherForecastByCity(city);
            if (weatherForecast == null) return null;
            _context.WeatherForecasts.Add(weatherForecast);
            _context.SaveChanges();
            return weatherForecast;
        }

        public async Task<IEnumerable<WeatherForecast>> AddAllWeatherForecasts()
        {
            IEnumerable<WeatherForecast> weatherForecasts = new List<WeatherForecast>();
            foreach (string city in cities)
            {
                WeatherForecast weatherForecast = await AddWeatherForecastByCity(city);
                if (weatherForecast == null)
                {
                    Console.WriteLine("Failed retrieving weather forecast from api");
                    continue;
                }
                weatherForecasts = weatherForecasts.Append(weatherForecast);
            }
            return weatherForecasts;
        }

        public void DeleteAllWeatherForecasts()
        {
            _context.WeatherForecasts.RemoveRange(_context.WeatherForecasts);
            _context.SaveChanges();
        }
    }
}
