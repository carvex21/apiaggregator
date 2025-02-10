using System.Text.Json;
using Aggregator.Api.Models;
using Microsoft.Extensions.Options;

namespace Aggregator.Api.Services;

public class NewsService : INewsService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public NewsService(HttpClient httpClient, IOptions<ApiSettings> settings)
    {
        _httpClient = httpClient;
        _apiKey = settings.Value.NewsApiKey ?? throw new ArgumentNullException("News API Key missing");

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Aggregator.Api/1.0");
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
    }


    public async Task<List<NewsArticle>> GetNewsAsync(string query)
    {
        try
        {
            string url = $"?q={query}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            string errorDetails = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"News API request failed: {response.StatusCode}- {errorDetails}");
            }

            string json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("articles", out var articles) || articles.GetArrayLength() == 0)
            {
                throw new KeyNotFoundException("No news articles found.");
            }

            var newsList = new List<NewsArticle>();

            foreach (var article in articles.EnumerateArray())
            {
                newsList.Add(new NewsArticle
                {
                    Author = article.TryGetProperty("author", out var authorProp)
                        ? authorProp.GetString()
                        : "Unknown Author",
                    Title = article.GetProperty("title").GetString() ?? "No Title",
                    Description = article.TryGetProperty("description", out var descProp)
                        ? descProp.GetString()
                        : "No Description",
                    Url = article.GetProperty("url").GetString() ?? "No Url",
                    PublishedAt = DateTime.TryParse(article.GetProperty("publishedAt").GetString(), out var date)
                        ? date
                        : DateTime.MinValue,
                    Content = article.TryGetProperty("content", out var contentProp)
                        ? contentProp.GetString()
                        : "No Content"
                });
            }

            return newsList;
        }
        catch (Exception)
        {
            return null;
        }
    }
}