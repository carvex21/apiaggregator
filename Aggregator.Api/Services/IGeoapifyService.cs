using Aggregator.Api.Models;

namespace Aggregator.Api.Services;

public interface IGeoapifyService
{
    Task<LocationData?> GetLocationDataAsync(string city);
}