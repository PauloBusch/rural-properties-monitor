using Analitycs.Domain.Entity;

namespace Analitycs.Domain.Interfaces;
public interface IIngressApiClient
{
    Task<List<SensorData>> GetSensorDataAsync(
        List<string> plotIds,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken
    );
}
