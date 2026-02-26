using Analitycs.Domain.Entity;
using Analitycs.Domain.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Analitycs.Data.Clients;

public class IngressApiClient : IIngressApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IKeycloakTokenService _keycloakTokenService;

    public IngressApiClient(HttpClient httpClient, IKeycloakTokenService keycloakTokenService)
    {
        _httpClient = httpClient;
        _keycloakTokenService = keycloakTokenService;
    }

    public async Task<List<SensorData>> GetSensorDataAsync(
        List<string> plotIds, DateTime startDate,
        DateTime endDate, CancellationToken cancellationToken)
    {
        if (plotIds == null || !plotIds.Any())
            throw new ArgumentException("plotIds cannot be empty");

        var accessToken = await _keycloakTokenService.GetAccessTokenAsync(cancellationToken);

        var query = $"plots/sensor-data?" +
                    $"plotIds={string.Join(",", plotIds)}" +
                    $"&startDate={startDate:O}" +
                    $"&endDate={endDate:O}";

        using var request = new HttpRequestMessage(HttpMethod.Get, query);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Error calling IngressApi: {response.StatusCode} - {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var result = JsonSerializer.Deserialize<List<SensorData>>(
            content,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return result ?? new List<SensorData>();
    }
}
