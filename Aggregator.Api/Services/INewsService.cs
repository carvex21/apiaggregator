using Aggregator.Api.Models;

namespace Aggregator.Api.Services;

public interface INewsService
{
    Task<List<NewsArticle>> GetNewsAsync(string city);
}