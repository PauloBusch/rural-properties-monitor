using Analitycs.Domain.Entity;

namespace Analitycs.Domain.Interfaces;
public interface IAnalyticsService
{
    Task<List<SensorData>> GetAnalyticsAsync(
            List<string> plotIds,
            DateTime start,
            DateTime end,
            string token,
            CancellationToken cancellationToken);
}
