using System.Text.Json;
using Aggregator.Api.Models;
using Microsoft.Extensions.Options;

namespace Aggregator.Api.Services;

public class GeoapifyService : IGeoapifyService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ICacheService _cacheService;

    public GeoapifyService(HttpClient httpClient, IOptions<ApiSettings> settings, ICacheService cacheService)
    {
        _httpClient = httpClient;
        _apiKey = settings.Value.GeoapifyApiKey ?? throw new ArgumentNullException("Geoapify API Key missing");
        _cacheService = cacheService;
    }

    public async Task<LocationData?> GetLocationDataAsync(string city)
    {
        var cachedLocation = _cacheService.GetLocationFromCache(city);
        if (cachedLocation != null)
        {
            return cachedLocation;
        }

        string url = $"?text={city}&lang=en&limit=1&type=city&format=json&apiKey={_apiKey}";
        HttpResponseMessage response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        string json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.GetProperty("results").GetArrayLength() == 0)
        {
            return null;
        }

        var location = root.GetProperty("results")[0];

        string? cityName = null;
        if (location.TryGetProperty("city", out var cityProp))
        {
            cityName = cityProp.GetString();
        }
        else if (location.TryGetProperty("district", out var districtProp))
        {
            cityName = districtProp.GetString();
        }
        else if (location.TryGetProperty("region", out var regionProp))
        {
            cityName = regionProp.GetString();
        }

        if (string.IsNullOrEmpty(cityName))
        {
            throw new KeyNotFoundException("No city, district, or region found in Geoapify response.");
        }

        if (!location.TryGetProperty("lat", out var latProp) ||
            !location.TryGetProperty("lon", out var lonProp) ||
            !location.TryGetProperty("place_id", out var placeIdProp))
        {
            throw new KeyNotFoundException("Missing expected keys in Geoapify response.");
        }

        var locationData = new LocationData
        {
            City = cityName,
            Latitude = latProp.GetDouble(),
            Longitude = lonProp.GetDouble(),
            PlaceId = placeIdProp.GetString()
        };

        _cacheService.SetLocationToCache(city, locationData);
        return locationData;
    }
}
