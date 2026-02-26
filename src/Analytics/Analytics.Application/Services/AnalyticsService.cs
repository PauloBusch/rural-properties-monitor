using Analitycs.Domain.Entity;
using Analitycs.Domain.Entity.Property;
using Analitycs.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Analytics.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IIngressApiClient _ingressApiClient;
    private readonly IPropertiesApiClient _propertiesApiClient;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AnalyticsService> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public AnalyticsService(
        IIngressApiClient ingressApiClient,
        IPropertiesApiClient propertiesApiClient,
        IDistributedCache cache,
        ILogger<AnalyticsService> logger)
    {
        _ingressApiClient = ingressApiClient ?? throw new ArgumentNullException(nameof(ingressApiClient));
        _propertiesApiClient = propertiesApiClient ?? throw new ArgumentNullException(nameof(propertiesApiClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<SensorData>> GetAnalyticsAsync(
        List<string> plotIds,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken)
    {
        
        # region cache key generation checking
        
            var cacheKey = GenerateCacheKey(plotIds, start, end);

            _logger.LogInformation("Checking cache for key {CacheKey}", cacheKey);

            var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogInformation("Cache hit for key {CacheKey}", cacheKey);
                return JsonSerializer.Deserialize<List<SensorData>>(cachedData)!;
            }

            _logger.LogInformation("Cache miss for key {CacheKey}. Calling IngressApi", cacheKey);
        
        #endregion
        
        var data = await _ingressApiClient.GetSensorDataAsync(
            plotIds,
            start,
            end,
            cancellationToken);

        var serialized = JsonSerializer.Serialize(data);

        await _cache.SetStringAsync(
            cacheKey,
            serialized,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration
            },
            cancellationToken);

        _logger.LogInformation("Data cached for key {CacheKey}", cacheKey);

        return data;
    }

    public async Task<List<Property>> GetPropertiesByProducerAsync(
        string producerId,
        string token,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"properties:{producerId}";

        _logger.LogInformation("Checking cache for key {CacheKey}", cacheKey);

        var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("Cache hit for key {CacheKey}", cacheKey);
            return JsonSerializer.Deserialize<List<Property>>(cachedData)!;
        }

        _logger.LogInformation("Cache miss for key {CacheKey}. Calling PropertiesApi", cacheKey);

        var data = await _propertiesApiClient.GetPropertiesByProducerAsync(
            producerId,
            token,
            cancellationToken);

        var serialized = JsonSerializer.Serialize(data);

        await _cache.SetStringAsync(
            cacheKey,
            serialized,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration
            },
            cancellationToken);

        _logger.LogInformation("Data cached for key {CacheKey}", cacheKey);

        return data;
    }

    private static string GenerateCacheKey(
        List<string> plotIds,
        DateTime start,
        DateTime end)
    {
        var orderedPlots = plotIds.OrderBy(p => p);
        var plots = string.Join("-", orderedPlots);

        return $"analytics:{plots}:{start:yyyyMMddHHmm}:{end:yyyyMMddHHmm}";
    }
}
