using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Aggregator.Api.Models;
using Microsoft.Extensions.Configuration;

namespace Aggregator.Api.Services;

public class PlacesService : IPlacesService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public PlacesService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["ApiSettings:GeoapifyApiKey"] ?? throw new ArgumentNullException("Geoapify API Key missing");
    }

    public async Task<List<PlaceData>> GetPlacesAsync(string placeId)
    {
        string url = $"?filter=place:{placeId}&categories=tourism.sights&limit=5&apiKey={_apiKey}";
        HttpResponseMessage response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return new List<PlaceData>(); // Επιστρέφουμε κενή λίστα αν αποτύχει η κλήση
        }

        string json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var places = new List<PlaceData>();
        foreach (var item in root.GetProperty("features").EnumerateArray())
        {
            places.Add(new PlaceData
            {
                Name = item.GetProperty("properties").GetProperty("name").GetString(),
                Category = item.GetProperty("properties").GetProperty("category").GetString(),
                Latitude = item.GetProperty("properties").GetProperty("lat").GetDouble(),
                Longitude = item.GetProperty("properties").GetProperty("lon").GetDouble()
            });
        }

        return places;
    }
}
