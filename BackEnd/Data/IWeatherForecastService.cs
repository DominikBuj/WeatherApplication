using BackEnd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Data
{
    public interface IWeatherForecastService
    {
        IEnumerable<WeatherForecast> GetWeatherForecastsByCity(string city);
        IEnumerable<WeatherForecast> GetAllWeatherForecasts();
        Task<WeatherForecast> AddWeatherForecastByCity(string city);
        Task<IEnumerable<WeatherForecast>> AddAllWeatherForecasts();
        void DeleteAllWeatherForecasts();
    }
}
