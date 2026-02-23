using Analytics.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Analytics.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly AnalyticsService _analyticsService;

    public AnalyticsController(AnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
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
        if (plotIds == null || !plotIds.Any())
            return BadRequest("plotIds é obrigatório");

        if (startDate >= endDate)
            return BadRequest("startDate deve ser anterior a endDate");

        var token = HttpContext.Request.Headers["Authorization"]
            .ToString()
            .Replace("Bearer ", "");

        var result = await _analyticsService.GetAnalyticsAsync(
            plotIds,
            startDate,
            endDate,
            token,
            cancellationToken);

        return Ok(result);
    }
}
