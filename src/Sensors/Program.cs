using Sensors;
using Microsoft.Extensions.Configuration;
using Sensors.Models;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var configRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var sensorConfig = new SensorConfig();
        configRoot.GetSection("SensorConfig").Bind(sensorConfig);

        var producer = new SensorDataProducer(sensorConfig.Kafka.Broker, sensorConfig.Kafka.Topic);

        await Task.WhenAll(
            sensorConfig.PlotIds.Select(plotId => RunEmulatorAsync(producer, sensorConfig, plotId))
        );
    }

    private static async Task RunEmulatorAsync(SensorDataProducer producer, SensorConfig sensorConfig, string plotId)
    {
        var emulator = new SensorEmulator(producer, plotId, sensorConfig.TriggerIntervalMs);
        Console.WriteLine($"Starting sensor emulator for plot '{plotId}' to Kafka broker '{sensorConfig.Kafka.Broker}', topic '{sensorConfig.Kafka.Topic}', interval '{sensorConfig.TriggerIntervalMs}' ms");
        await emulator.RunAsync();
    }
}
