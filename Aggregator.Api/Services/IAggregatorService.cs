using Aggregator.Api.Models;

namespace Aggregator.Api.Services;

public interface IAggregatorService
{
    Task<AggregatedData?> GetAggregatedDataAsync(string city);
}
