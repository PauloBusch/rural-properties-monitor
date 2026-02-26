using Analitycs.Domain.Interfaces;
using Analitycs.Domain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Analitycs.Data.Clients;

public class KeycloakTokenClient : IKeycloakTokenService
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakSettings _settings;
    private readonly ILogger<KeycloakTokenClient> _logger;

    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public KeycloakTokenClient(
        HttpClient httpClient,
        IOptions<KeycloakSettings> settings,
        ILogger<KeycloakTokenClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiry)
        {
            _logger.LogDebug("Using cached Keycloak token");
            return _cachedToken;
        }

        _logger.LogInformation("Requesting new access token from Keycloak at {TokenUrl}", _settings.TokenUrl);

        var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = _settings.GrantType,
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret
        });

        using var response = await _httpClient.PostAsync(_settings.TokenUrl, requestBody, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to obtain Keycloak token: {StatusCode} - {Error}", response.StatusCode, errorContent);
            throw new HttpRequestException(
                $"Failed to obtain Keycloak token: {response.StatusCode} - {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        var accessToken = root.GetProperty("access_token").GetString()
            ?? throw new InvalidOperationException("access_token not found in Keycloak response");

        var expiresIn = root.TryGetProperty("expires_in", out var expProp)
            ? expProp.GetInt32()
            : 300;

        // Cache with a safety margin of 30 seconds before actual expiry
        _cachedToken = accessToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 30);

        _logger.LogInformation("Keycloak access token obtained successfully. Expires in {ExpiresIn}s", expiresIn);

        return accessToken;
    }
}

