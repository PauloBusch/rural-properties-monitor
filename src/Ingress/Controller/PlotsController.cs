using Microsoft.AspNetCore.Authorization;
using IngressApi.Repositories;
using IngressApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IngressApi.Controller;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PlotsController : ControllerBase
{
    private readonly ISensorDataService _sensorDataService;
    private readonly ISensorDataRepository _repository;

    public PlotsController(ISensorDataService sensorDataService, ISensorDataRepository repository)
    {
        _sensorDataService = sensorDataService;
        _repository = repository;
    }

    /// <summary>
    /// Consulta dados de sensores agregados por hora e período
    /// </summary>
    /// <param name="plotIds">Lista de IDs dos talhões (separados por vírgula). Ex: plot-001,plot-002</param>
    /// <param name="startDate">Data/hora de início (ex: 2026-02-25 ou 2026-02-25T00:00:00Z)</param>
    /// <param name="endDate">Data/hora de fim (ex: 2026-02-25 ou 2026-02-25T23:59:59Z)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [HttpGet("sensor-data")]
    public async Task<IActionResult> GetSensorData(
        [FromQuery] string plotIds,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(plotIds))
            return BadRequest("plotIds é obrigatório. Exemplo: plot-001,plot-002");
        
        if (startDate > endDate)
            return BadRequest("startDate deve ser anterior ou igual a endDate");

        // Normaliza as datas para UTC
        var queryStartDate = startDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(startDate, DateTimeKind.Utc)
            : startDate.ToUniversalTime();
        
        var queryEndDate = endDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(endDate, DateTimeKind.Utc)
            : endDate.ToUniversalTime();
        
        if (queryEndDate.TimeOfDay == TimeSpan.Zero)
        {
            queryEndDate = queryEndDate.AddDays(1);
        }
        
        var plotIdList = plotIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(id => id.Trim())
                                .ToList();

        var result = await _sensorDataService.GetAggregatedDataAsync(
            plotIdList, queryStartDate, queryEndDate, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Endpoint de diagnóstico - retorna os dados mais recentes do InfluxDB
    /// para verificar se os dados estão sendo persistidos corretamente
    /// </summary>
    /// <param name="limit">Quantidade máxima de registros (padrão: 10)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    [HttpGet("sensor-data/debug")]
    public async Task<IActionResult> GetDebugData(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var data = await _repository.GetRecentDataAsync(limit, cancellationToken);

        return Ok(new
        {
            TotalRecords = data.Count,
            Message = data.Count == 0
                ? "Nenhum dado encontrado no InfluxDB nos últimos 30 dias. Verifique se o KafkaConsumer está salvando corretamente."
                : $"Encontrados {data.Count} registros. Use os PlotIds e timestamps abaixo para suas consultas.",
            PlotIdsDisponiveis = data.Select(d => d.PlotId).Distinct().ToList(),
            RangeTimestamps = data.Count > 0
                ? new { Min = data.Min(d => d.Timestamp), Max = data.Max(d => d.Timestamp) }
                : null,
            Data = data
        });
    }
}
