using Aggregator.Api.Models;

namespace Aggregator.Api.Services;

public interface IPlacesService
{
    Task<List<PlaceData>> GetPlacesAsync(string placeId);
}
