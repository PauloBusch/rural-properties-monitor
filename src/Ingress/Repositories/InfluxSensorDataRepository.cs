using IngressApi.Models;
using Sensors.Models;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace IngressApi.Repositories
{
    public class InfluxSensorDataRepository : ISensorDataRepository
    {
        private readonly InfluxDbConfig _config;
        private readonly InfluxDBClient _client;
        private readonly ILogger<InfluxSensorDataRepository> _logger;

        public InfluxSensorDataRepository(InfluxDbConfig config, ILogger<InfluxSensorDataRepository> logger)
        {
            _config = config;
            _client = new InfluxDBClient(_config.Url, _config.Token);
            _logger = logger;
        }

        public async Task SaveAsync(SensorDataPayload data, CancellationToken cancellationToken = default)
        {
            try
            {
                var utcTimestamp = data.Timestamp.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(data.Timestamp, DateTimeKind.Utc)
                    : data.Timestamp.ToUniversalTime();

                var point = PointData
                    .Measurement("sensor_data")
                    .Tag("plotId", data.PlotId)
                    .Field("soilMoisture", data.SoilMoisture)
                    .Field("temperature", data.Temperature)
                    .Field("precipitation", data.Precipitation)
                    .Timestamp(utcTimestamp, WritePrecision.Ns);

                _logger.LogInformation(
                    "Saving to InfluxDB - PlotId: {PlotId}, Timestamp: {Timestamp}, Bucket: {Bucket}",
                    data.PlotId, utcTimestamp.ToString("o"), _config.Bucket);

                var writeApi = _client.GetWriteApiAsync();
                await writeApi.WritePointAsync(point, _config.Bucket, _config.Org, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save data to InfluxDB for PlotId: {PlotId}", data.PlotId);
                throw;
            }
        }

        public async Task<List<SensorDataPayload>> GetByPlotIdsAsync(
            List<string> plotIds, 
            DateTime startDate, 
            DateTime endDate, 
            CancellationToken cancellationToken = default)
        {
            var results = new List<SensorDataPayload>();
            var queryApi = _client.GetQueryApi();

            var plotIdFilter = string.Join(" or ", plotIds.Select(id => $"r.plotId == \"{id}\""));

            var startUtc = startDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            var stopUtc = endDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

            var fluxQuery = $@"
                from(bucket: ""{_config.Bucket}"")
                |> range(start: {startUtc}, stop: {stopUtc})
                |> filter(fn: (r) => r._measurement == ""sensor_data"")
                |> filter(fn: (r) => {plotIdFilter})
                |> pivot(rowKey: [""_time"", ""plotId""], columnKey: [""_field""], valueColumn: ""_value"")
            ";

            _logger.LogInformation("Executing Flux query: {Query}", fluxQuery);

            var tables = await queryApi.QueryAsync(fluxQuery, _config.Org, cancellationToken);

            foreach (var table in tables)
            {
                foreach (var record in table.Records)
                {
                    results.Add(new SensorDataPayload
                    {
                        PlotId = record.Values["plotId"]?.ToString() ?? string.Empty,
                        SoilMoisture = Convert.ToDouble(record.Values["soilMoisture"] ?? 0),
                        Temperature = Convert.ToDouble(record.Values["temperature"] ?? 0),
                        Precipitation = Convert.ToDouble(record.Values["precipitation"] ?? 0),
                        Timestamp = record.GetTimeInDateTime() ?? DateTime.UtcNow
                    });
                }
            }

            _logger.LogInformation("InfluxDB query returned {Count} records for plotIds: {PlotIds}", results.Count, string.Join(", ", plotIds));

            return results;
        }

        public async Task<List<SensorDataPayload>> GetRecentDataAsync(
            int limit = 10,
            CancellationToken cancellationToken = default)
        {
            var results = new List<SensorDataPayload>();
            var queryApi = _client.GetQueryApi();

            // Busca dados amplos para diagnóstico (inclui futuro para cobrir diferenças de timezone)
            var futureStop = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var fluxQuery = $@"
                from(bucket: ""{_config.Bucket}"")
                |> range(start: -30d, stop: {futureStop})
                |> filter(fn: (r) => r._measurement == ""sensor_data"")
                |> pivot(rowKey: [""_time"", ""plotId""], columnKey: [""_field""], valueColumn: ""_value"")
                |> sort(columns: [""_time""], desc: true)
                |> limit(n: {limit})
            ";

            _logger.LogInformation("Executing diagnostic Flux query: {Query}", fluxQuery);

            var tables = await queryApi.QueryAsync(fluxQuery, _config.Org, cancellationToken);

            foreach (var table in tables)
            {
                foreach (var record in table.Records)
                {
                    results.Add(new SensorDataPayload
                    {
                        PlotId = record.Values["plotId"]?.ToString() ?? string.Empty,
                        SoilMoisture = Convert.ToDouble(record.Values["soilMoisture"] ?? 0),
                        Temperature = Convert.ToDouble(record.Values["temperature"] ?? 0),
                        Precipitation = Convert.ToDouble(record.Values["precipitation"] ?? 0),
                        Timestamp = record.GetTimeInDateTime() ?? DateTime.UtcNow
                    });
                }
            }

            _logger.LogInformation("Diagnostic query returned {Count} records", results.Count);

            return results;
        }
    }
}