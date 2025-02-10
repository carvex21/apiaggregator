namespace Aggregator.Api.Models;

public class AggregatedData
{
    public LocationInfo? Location { get; set; }
    public WeatherData? Weather { get; set; }
    public List<NewsArticle>? News { get; set; }
    public List<PlaceData>? Places { get; set; }
}