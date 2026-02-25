using System;
using System.Threading.Tasks;
using Sensors.Models;

namespace Sensors
{
    public class SensorEmulator
    {
        private readonly SensorDataProducer _producer;
        private readonly Random _random = new();
        private readonly string _plotId;


        private readonly int _intervalMs;

        public SensorEmulator(SensorDataProducer producer, string plotId, int intervalMs)
        {
            _producer = producer;
            _plotId = plotId;
            _intervalMs = intervalMs;
        }

        public async Task RunAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var data = new SensorDataRequest
                {
                    PlotId = _plotId,
                    SoilMoisture = Math.Round(_random.NextDouble() * 100, 2),
                    Temperature = Math.Round(_random.NextDouble() * 40 - 10, 2),
                    Precipitation = Math.Round(_random.NextDouble() * 20, 2),
                    Timestamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
                };
                await _producer.ProduceAsync(data);
                Console.WriteLine($"Sent: {System.Text.Json.JsonSerializer.Serialize(data)}");
                try
                {
                    await Task.Delay(_intervalMs, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}
