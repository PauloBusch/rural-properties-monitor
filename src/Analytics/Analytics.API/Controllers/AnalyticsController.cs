using Analitycs.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Analytics.API.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Consulta dados agregados de sensores via IngressApi
    /// </summary>
    [HttpGet("sensor-data")]
    public async Task<IActionResult> GetSensorData(
        [FromQuery] List<string> plotIds,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Request received for sensor data. Plots: {PlotIds}, StartDate: {StartDate}, EndDate: {EndDate}",
            plotIds,
            startDate,
            endDate);

        if (plotIds == null || !plotIds.Any())
        {
            _logger.LogWarning("Invalid request: plotIds is empty");
            return BadRequest("plotIds is required");
        }

        if (startDate >= endDate)
        {
            _logger.LogWarning("Invalid request: startDate >= endDate");
            return BadRequest("startDate must be earlier than endDate");
        }

        var token = HttpContext.Request.Headers["Authorization"]
            .ToString()
            .Replace("Bearer ", "");

        var result = await _analyticsService.GetAnalyticsAsync(
            plotIds,
            startDate,
            endDate,
            cancellationToken);

        _logger.LogInformation("Sensor data successfully retrieved");

        return Ok(result);
    }

    /// <summary>
    /// Consulta propriedades e talhões via PropertiesApi usando producerId
    /// </summary>
    [HttpGet("properties/producer/{producerId}")]
    public async Task<IActionResult> GetPropertiesByProducer(
        string producerId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Request received for properties of producer {ProducerId}", producerId);

        if (string.IsNullOrWhiteSpace(producerId))
        {
            _logger.LogWarning("Invalid request: producerId is empty");
            return BadRequest("producerId is required");
        }

        var token = HttpContext.Request.Headers["Authorization"]
            .ToString()
            .Replace("Bearer ", "");

        var result = await _analyticsService.GetPropertiesByProducerAsync(
            producerId,
            token,
            cancellationToken);

        _logger.LogInformation("Properties successfully retrieved for producer {ProducerId}", producerId);

        return Ok(result);
    }
}
