using Microsoft.AspNetCore.Authorization;
using IngressApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IngressApi.Controller;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PlotsController : ControllerBase
{
    private readonly ISensorDataService _sensorDataService;

    public PlotsController(ISensorDataService sensorDataService)
    {
        _sensorDataService = sensorDataService;
    }

    /// <summary>
    /// Consulta dados de sensores agregados por hora e período
    /// </summary>
    /// <param name="plotIds">Lista de IDs dos talhões (separados por vírgula)</param>
    /// <param name="startDate">Data de início</param>
    /// <param name="endDate">Data de fim</param>
    [HttpGet("sensor-data")]
    public async Task<IActionResult> GetSensorData(
        [FromQuery] string plotIds,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(plotIds))
            return BadRequest("plotIds é obrigatório");
        
        if (startDate >= endDate)
            return BadRequest("startDate deve ser anterior a endDate");
        
        // Converte a string de plotIds em uma lista
        var plotIdList = plotIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(id => id.Trim())
                                .ToList();

        var result = await _sensorDataService.GetAggregatedDataAsync(
            plotIdList, startDate, endDate, cancellationToken);

        return Ok(result);
    }
}
