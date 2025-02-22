﻿using System.Text.Json;
using Aggregator.Api.Models;
using Microsoft.Extensions.Options;

namespace Aggregator.Api.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public WeatherService(HttpClient httpClient, IOptions<ApiSettings> settings)
    {
        _httpClient = httpClient;
        _apiKey = settings.Value.WeatherApiKey ?? throw new ArgumentNullException("Weather API Key missing");
    }

    public async Task<WeatherData?> GetWeatherAsync(double latitude, double longitude)
    {
        try
        {
            string url = $"?lat={latitude}&lon={longitude}&units=metric&appid={_apiKey}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new WeatherData
            {
                Temperature = root.GetProperty("main").GetProperty("temp").GetDouble(),
                Condition = root.GetProperty("weather")[0].GetProperty("description").GetString(),
                WindSpeed = root.GetProperty("wind").GetProperty("speed").GetDouble(),
                Humidity = root.GetProperty("main").GetProperty("humidity").GetInt32()
            };
        }
        catch (Exception)
        {
            return null;
        }
    }
}