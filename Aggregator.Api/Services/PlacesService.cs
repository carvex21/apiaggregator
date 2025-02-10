using System.Text.Json;
using Aggregator.Api.Models;
using Microsoft.Extensions.Options;

namespace Aggregator.Api.Services;

public class PlacesService : IPlacesService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public PlacesService(HttpClient httpClient, IOptions<ApiSettings> settings)
    {
        _httpClient = httpClient;
        _apiKey = settings.Value.GeoapifyApiKey ?? throw new ArgumentNullException("Places API Key missing");
    }

    public async Task<List<PlaceData>> GetPlacesAsync(string placeId)
    {
        try
        {
            string url = $"?filter=place:{placeId}&categories=tourism.sights&limit=5&apiKey={_apiKey}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Places API request failed: {response.StatusCode}");
            }

            string json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("features", out var features) || features.GetArrayLength() == 0)
            {
                throw new KeyNotFoundException("No places found in Places API response.");
            }

            var places = new List<PlaceData>();

            foreach (var feature in features.EnumerateArray())
            {
                var properties = feature.GetProperty("properties");

                string? name = properties.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "Unknown";

                string? category = properties.TryGetProperty("categories", out var categoriesProp) &&
                                   categoriesProp.GetArrayLength() > 0
                    ? categoriesProp[0].GetString()
                    : "Unknown";

                string? address = properties.TryGetProperty("formatted", out var addressProp)
                    ? addressProp.GetString()
                    : "No Address";

                places.Add(new PlaceData
                {
                    Name = name,
                    Category = category,
                    Address = address
                });
            }

            return places;
        }
        catch (Exception)
        {
            return null;
        }
    }
}