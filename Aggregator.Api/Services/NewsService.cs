using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Aggregator.Api.Models;
using Microsoft.Extensions.Configuration;

namespace Aggregator.Api.Services;

public class NewsService : INewsService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public NewsService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["ApiSettings:NewsApiKey"] ?? throw new ArgumentNullException("News API Key missing");
    }

    public async Task<List<NewsArticle>> GetNewsAsync(string city)
    {
        string url = $"?q={city}&apiKey={_apiKey}";
        HttpResponseMessage response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return new List<NewsArticle>(); // Επιστρέφουμε κενή λίστα αν αποτύχει η κλήση
        }

        string json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var articles = new List<NewsArticle>();
        foreach (var item in root.GetProperty("articles").EnumerateArray())
        {
            articles.Add(new NewsArticle
            {
                Title = item.GetProperty("title").GetString(),
                Source = item.GetProperty("source").GetProperty("name").GetString(),
                Url = item.GetProperty("url").GetString(),
                PublishedAt = item.GetProperty("publishedAt").GetString()
            });
        }

        return articles;
    }
}