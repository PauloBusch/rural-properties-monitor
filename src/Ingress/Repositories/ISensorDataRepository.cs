using Sensors.Models;

namespace IngressApi.Repositories
{
    public interface ISensorDataRepository
    {
        Task SaveAsync(SensorDataPayload data, CancellationToken cancellationToken = default);
        
        Task<List<SensorDataPayload>> GetByPlotIdsAsync(
            List<string> plotIds, 
            DateTime startDate, 
            DateTime endDate, 
            CancellationToken cancellationToken = default);

        Task<List<SensorDataPayload>> GetRecentDataAsync(
            int limit = 10,
            CancellationToken cancellationToken = default);
    }
}