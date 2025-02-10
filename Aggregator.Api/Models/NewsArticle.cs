namespace Aggregator.Api.Models;

public class NewsArticle
{
    public string? Author { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public string? Content { get; set; }
}
