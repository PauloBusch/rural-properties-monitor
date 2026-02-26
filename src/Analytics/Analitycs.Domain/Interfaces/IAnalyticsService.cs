using Analitycs.Domain.Entity;
using Analitycs.Domain.Entity.Property;

namespace Analitycs.Domain.Interfaces;
public interface IAnalyticsService
{
    Task<List<SensorData>> GetAnalyticsAsync(
            List<string> plotIds,
            DateTime start,
            DateTime end,
            string token,
            CancellationToken cancellationToken);

    Task<List<Property>> GetPropertiesByProducerAsync(
        string producerId,
        string token,
        CancellationToken cancellationToken);
}
