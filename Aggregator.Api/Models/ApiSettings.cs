namespace Aggregator.Api.Models;

public class ApiSettings
{
    public string GeoapifyApiKey { get; set; } = string.Empty;
    public string GeoapifyBaseUrl { get; set; } = string.Empty;
    public string PlacesApiBaseUrl { get; set; } = string.Empty;
    public string WeatherApiKey { get; set; } = string.Empty;
    public string WeatherBaseUrl { get; set; } = string.Empty;
    public string NewsApiKey { get; set; } = string.Empty;
    public string NewsBaseUrl { get; set; } = string.Empty;
}
