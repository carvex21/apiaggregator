using Aggregator.Api.Models;

namespace Aggregator.Api.Services;

public interface ICacheService
{
    LocationData? GetLocationFromCache(string city);

    void SetLocationToCache(string city, LocationData locationData);
}
