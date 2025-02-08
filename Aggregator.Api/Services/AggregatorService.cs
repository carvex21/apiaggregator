using System.Collections.Generic;
using System.Threading.Tasks;
using Aggregator.Api.Models;

namespace Aggregator.Api.Services;

public class AggregatorService : IAggregatorService
{
    private readonly IGeoapifyService _geoapifyService;
    private readonly IWeatherService _weatherService;
    private readonly INewsService _newsService;
    private readonly IPlacesService _placesService;

    public AggregatorService(IGeoapifyService geoapifyService, IWeatherService weatherService, INewsService newsService, IPlacesService placesService)
    {
        _geoapifyService = geoapifyService;
        _weatherService = weatherService;
        _newsService = newsService;
        _placesService = placesService;
    }

    public async Task<AggregatedData?> GetAggregatedDataAsync(string city)
    {
        var locationData = await _geoapifyService.GetLocationDataAsync(city);
        if (locationData == null)
        {
            return null;
        }

        var weatherTask = _weatherService.GetWeatherAsync(locationData.Latitude, locationData.Longitude);
        var newsTask = _newsService.GetNewsAsync(city);
        var placesTask = _placesService.GetPlacesAsync(locationData.PlaceId);

        await Task.WhenAll(weatherTask, newsTask, placesTask);

        return new AggregatedData
        {
            Location = new LocationInfo
            {
                City = locationData.City,
                Latitude = locationData.Latitude,
                Longitude = locationData.Longitude
            },
            Weather = weatherTask.Result,
            News = newsTask.Result,
            Places = placesTask.Result
        };
    }
}