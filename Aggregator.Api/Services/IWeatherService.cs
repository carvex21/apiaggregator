using Aggregator.Api.Models;

namespace Aggregator.Api.Services;

public interface IWeatherService
{
    Task<WeatherData?> GetWeatherAsync(double latitude, double longitude);
}
