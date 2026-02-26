using Analytics.Application.DTOs;

namespace Analitycs.Application.Interfaces;
public interface IDashboardService
{
    Task<ProducerDashboardResponse> GetDashboardAsync(
        string producerId,
        string token,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken);
}
