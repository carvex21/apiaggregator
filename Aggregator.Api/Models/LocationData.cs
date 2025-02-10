namespace Aggregator.Api.Models;

public class LocationData
{
    public string? City { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? PlaceId { get; set; }

    public LocationData(string? city, double latitude, double longitude, string? placeId = null)
    {
        City = city;
        Latitude = latitude;
        Longitude = longitude;
        PlaceId = placeId;
    }
}