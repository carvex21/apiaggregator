namespace Aggregator.Api.Models;

public class WeatherData
{
    public double Temperature { get; set; }
    public string? Condition { get; set; }
    public double WindSpeed { get; set; }
    public int Humidity { get; set; }
}
