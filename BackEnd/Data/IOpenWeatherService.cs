using BackEnd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Data
{
    public interface IOpenWeatherService
    {
        Task<WeatherForecast> GetWeatherForecastByCity(string city);
        Task<IEnumerable<WeatherForecast>> GetAllWeatherForecasts();
    }
}
