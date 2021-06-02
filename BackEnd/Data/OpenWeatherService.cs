using BackEnd.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BackEnd.Data
{
    public class OpenWeatherService : IOpenWeatherService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string openWeatherApiKey;
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

        public OpenWeatherService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            openWeatherApiKey = _configuration.GetValue<string>("OpenWeatherApiKey");
        }

        private float GetFloatValue(dynamic dynamicValue)
        {
            string value = dynamicValue;
            return Convert.ToSingle(value);
        }

        private WeatherForecast GetWeatherForecast(string city, string responseString)
        {
            dynamic reponseObject = JsonConvert.DeserializeObject(responseString);
            return new WeatherForecast
            {
                City = city,
                Date = DateTime.Now,
                Temperature = GetFloatValue(reponseObject.main.temp) - (float)273.15,
                Pressure = GetFloatValue(reponseObject.main.pressure),
                Humidity = GetFloatValue(reponseObject.main.humidity),
                Precipitation = GetFloatValue(reponseObject.visibility),
                WindSpeed = GetFloatValue(reponseObject.wind.speed),
                WindDirection = GetFloatValue(reponseObject.wind.deg)
            };
        }

        public async Task<WeatherForecast> GetWeatherForecastByCity(string city)
        {
            string requestUrl = $"api.openweathermap.org/data/2.5/weather?q={city}&appid={openWeatherApiKey}";
            UriBuilder uriBuilder = new UriBuilder(requestUrl);
            HttpClient httpClient = _httpClientFactory.CreateClient();
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(uriBuilder.Uri);
            string responseString = await httpResponseMessage.Content.ReadAsStringAsync();
            if (!httpResponseMessage.IsSuccessStatusCode) return null;
            return GetWeatherForecast(city, responseString);
        }

        public async Task<IEnumerable<WeatherForecast>> GetAllWeatherForecasts()
        {
            IEnumerable<WeatherForecast> weatherForecasts = new List<WeatherForecast>();
            foreach (string city in cities)
            {
                WeatherForecast weatherForecast = await GetWeatherForecastByCity(city);
                weatherForecasts = weatherForecasts.Append(weatherForecast);
            }
            return weatherForecasts;
        }
    }
}
