namespace Aggregator.Api.Models;

public class LocationInfo
{
    public string? City { get; }
    public double Latitude { get; }
    public double Longitude { get; }

    public LocationInfo(string? city, double latitude, double longitude)
    {
        City = city;
        Latitude = latitude;
        Longitude = longitude;
    }
}
