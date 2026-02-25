using Analitycs.Domain.Entity;
using Analitycs.Domain.Interfaces;
using Analytics.API._Common;
using Analytics.IntegrationTests._Common;
using Microsoft.Extensions.Caching.Distributed;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Analytics.IntegrationTests.Controllers;
public class AnalyticsControllerTests : ControllerTestBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IDistributedCache _cache;

    public AnalyticsControllerTests(AnalyticsFixture fixture) : base(fixture, "analytics")
    {
        _analyticsService = GetService<IAnalyticsService>();
        _cache = GetService<IDistributedCache>();
    }

    [Fact]
    public async Task ShouldGetSensorDataSuccessfullyAsync()
    {
        // Arrange
        var plotIds = new List<string> { "plot1", "plot2" };
        var startDate = DateTime.UtcNow.AddHours(-1);
        var endDate = DateTime.UtcNow;

        var uri = new Uri($"{Uri}/sensor-data?plotIds={string.Join("&plotIds=", plotIds)}&startDate={startDate:o}&endDate={endDate:o}");

        // Act
        var (httpMessage, response) = await Requester.GetAsync<List<SensorData>>(uri, ct: CancellationToken);

        // Assert
        httpMessage.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.ShouldNotBeNull();
        response.Count.ShouldBeGreaterThan(0);

        // Verifica se os dados foram cacheados
        var cacheKey = $"analytics:{string.Join("-", plotIds.OrderBy(p => p))}:{startDate:yyyyMMddHHmm}:{endDate:yyyyMMddHHmm}";
        var cached = await _cache.GetStringAsync(cacheKey, CancellationToken);
        cached.ShouldNotBeNull();

        var cachedData = JsonSerializer.Deserialize<List<SensorData>>(cached)!;
        cachedData.Count.ShouldBe(response.Count);
    }

    [Fact]
    public async Task ShouldRejectWhenPlotIdsIsEmptyAsync()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddHours(-1);
        var endDate = DateTime.UtcNow;
        var uri = new Uri($"{Uri}/sensor-data?startDate={startDate:o}&endDate={endDate:o}");

        // Act
        var (httpMessage, response) = await Requester.GetAsync<ErrorResponse>(uri, ct: CancellationToken);

        // Assert
        httpMessage.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Errors.ShouldContain(e => e.Message.Contains("plotIds is required"));
    }

    [Fact]
    public async Task ShouldRejectWhenStartDateAfterEndDateAsync()
    {
        // Arrange
        var plotIds = new List<string> { "plot1" };
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddHours(-1);
        var uri = new Uri($"{Uri}/sensor-data?plotIds={string.Join("&plotIds=", plotIds)}&startDate={startDate:o}&endDate={endDate:o}");

        // Act
        var (httpMessage, response) = await Requester.GetAsync<ErrorResponse>(uri, ct: CancellationToken);

        // Assert
        httpMessage.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.ShouldNotBeNull();
        response.Errors.ShouldContain(e => e.Message.Contains("plotIds is required"));
    }

    [Fact]
    public async Task ShouldReturnCachedDataOnSubsequentRequestsAsync()
    {
        // Arrange
        var plotIds = new List<string> { "plot1", "plot2" };
        var startDate = DateTime.UtcNow.AddHours(-1);
        var endDate = DateTime.UtcNow;

        var uri = new Uri($"{Uri}/sensor-data?plotIds={string.Join("&plotIds=", plotIds)}&startDate={startDate:o}&endDate={endDate:o}");

        // Primeira requisição para preencher o cache
        var (_, firstResponse) = await Requester.GetAsync<List<SensorData>>(uri, ct: CancellationToken);

        // Segunda requisição, deve vir do cache
        var (_, secondResponse) = await Requester.GetAsync<List<SensorData>>(uri, ct: CancellationToken);

        // Assert
        secondResponse.ShouldNotBeNull();
        secondResponse.Count.ShouldBe(firstResponse.Count);
    }
}
