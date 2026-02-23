using Analitycs.Domain.Entity;
using Analitycs.Domain.Interfaces;
using Analytics.Application.DTOs;

namespace Analytics.Application.Services;
public class AnalyticsService
{
    private readonly IIngressApiClient _ingressApiClient;

    public AnalyticsService(IIngressApiClient ingressApiClient)
    {
        _ingressApiClient = ingressApiClient ?? throw new ArgumentNullException(nameof(ingressApiClient));
    }
    //converter de entity para dto no automapper
    public async Task<List<SensorData>> GetAnalyticsAsync(
        List<string> plotIds,
        DateTime start,
        DateTime end,
        string token,
        CancellationToken cancellationToken)
    {
        var data = await _ingressApiClient.GetSensorDataAsync(
            plotIds,
            start,
            end,
            token,
            cancellationToken);

        return data;
    }
}
