using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Aggregator.Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Aggregator.Api.Services;

public class GeoapifyService : IGeoapifyService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeoapifyService(HttpClient httpClient, IOptions<ApiSettings> settings)
    {
        _httpClient = httpClient;
        _apiKey = settings.Value.GeoapifyApiKey ?? throw new ArgumentNullException("Weather API Key missing");

    }

    public async Task<LocationData?> GetLocationDataAsync(string city)
    {
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

        return new LocationData
        {
            City = location.GetProperty("city").GetString(),
            Latitude = location.GetProperty("lat").GetDouble(),
            Longitude = location.GetProperty("lon").GetDouble(),
            PlaceId = location.GetProperty("place_id").GetString()
        };
    }
}
