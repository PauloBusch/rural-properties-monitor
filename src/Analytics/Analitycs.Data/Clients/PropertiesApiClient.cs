using Analitycs.Domain.Entity.Property;
using Analitycs.Domain.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Analitycs.Data.Clients;
public class PropertiesApiClient : IPropertiesApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IKeycloakTokenService _keycloakTokenService;
    public PropertiesApiClient(HttpClient httpClient, IKeycloakTokenService keycloakTokenService)
    {
        _httpClient = httpClient;
        _keycloakTokenService = keycloakTokenService;
    }
    public async Task<List<Property>> GetPropertiesByProducerAsync(string producerId, string token, CancellationToken cancellationToken)
    {
        var accessToken = await _keycloakTokenService.GetAccessTokenAsync(cancellationToken);

        var url = $"https://localhost:44325/api/Properties/producer/{producerId}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);        

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Error calling PropertiesApi: {response.StatusCode} - {errorContent}");
        } 
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken); 
        var result = JsonSerializer.Deserialize<List<Property>>(
            content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        return result ?? new List<Property>();
    }
}
