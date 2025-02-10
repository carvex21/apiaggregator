using Aggregator.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace Aggregator.Api.Controllers;

[ApiController]
[Route("api/aggregator")]
public class AggregatorController : ControllerBase
{
    private readonly IAggregatorService _aggregatorService;

    public AggregatorController(IAggregatorService aggregatorService)
    {
        _aggregatorService = aggregatorService;
    }

    [HttpGet("data")]
    public async Task<IActionResult> GetAggregatedData([FromQuery] string city)
    {
        var aggregatedData = await _aggregatorService.GetAggregatedDataAsync(city);
        if (aggregatedData == null)
        {
            return NotFound("No data found for this city.");
        }
        return Ok(aggregatedData);
    }
}
