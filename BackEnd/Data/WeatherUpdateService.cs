using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BackEnd.Data
{
    internal interface IWeatherUpdateService
    {
        Task DoWork(CancellationToken cancellationToken);
    }

    public class WeatherUpdateService : IWeatherUpdateService
    {
        private readonly IWeatherForecastService _weatherForecastService;

        public WeatherUpdateService(IWeatherForecastService weatherForecastService)
        {
            _weatherForecastService = weatherForecastService;
        }

        public async Task DoWork(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await _weatherForecastService.AddAllWeatherForecasts();
                await Task.Delay(57600000, cancellationToken);
            }
        }
    }
}