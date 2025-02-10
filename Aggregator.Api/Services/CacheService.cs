using System.Collections.Concurrent;
using Aggregator.Api.Models;

namespace Aggregator.Api.Services;

public class CacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, LocationData> _cache = new();

    public LocationData? GetLocationFromCache(string city)
    {
        _cache.TryGetValue(city.ToLower(), out var location);
        return location;
    }

    public void SetLocationToCache(string city, LocationData locationData)
    {
        _cache[city.ToLower()] = locationData;
    }
}
