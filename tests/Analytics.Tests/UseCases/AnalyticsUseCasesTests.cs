using Analitycs.Domain.Interfaces;
using Analytics.Application.Services;
using Analytics.Tests._Common;
using Analytics.Tests.Factories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using System.Text;
using System.Text.Json;

namespace Analytics.Tests.Services;

public class AnalyticsServiceTests : UseCaseTestBase<AnalyticsService>
{
    private readonly IIngressApiClient _ingressApiClient;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsServiceTests(AnalyticsFixture fixture) : base(fixture)
    {
        _ingressApiClient = GetMock<IIngressApiClient>();
        _cache = GetMock<IDistributedCache>();
        _logger = GetMock<ILogger<AnalyticsService>>();
    }

    [Fact]
    public async Task ShouldReturnDataFromCacheWhenCacheHitAsync()
    {
        var factory = new ModelFactory();
        var expectedData = factory.SensorDataList;
        var serialized = JsonSerializer.Serialize(expectedData);
        var bytes = Encoding.UTF8.GetBytes(serialized);

        var ingressApiClient = Substitute.For<IIngressApiClient>();
        var cache = Substitute.For<IDistributedCache>();
        var logger = Substitute.For<ILogger<AnalyticsService>>();

        cache.GetAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<byte[]?>(bytes));

        var service = new AnalyticsService(
            ingressApiClient,
            cache,
            logger);

        var result = await service.GetAnalyticsAsync(
            factory.PlotIds,
            factory.StartDate,
            factory.EndDate,
            factory.Token,
            CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(expectedData.Count);
    }

    [Fact]
    public async Task ShouldCallIngressApiAndCacheWhenCacheMissAsync()
    {
        var factory = new ModelFactory();
        var expectedData = factory.SensorDataList;

        _cache.GetAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<byte[]?>(null));

        _ingressApiClient
            .GetSensorDataAsync(
                Arg.Any<List<string>>(),
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(expectedData);

        var result = await UseCase.GetAnalyticsAsync(
            factory.PlotIds,
            factory.StartDate,
            factory.EndDate,
            factory.Token,
            CancellationToken);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(expectedData.Count);

        await _ingressApiClient.Received(1)
            .GetSensorDataAsync(
                Arg.Any<List<string>>(),
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>(),
                factory.Token,
                Arg.Any<CancellationToken>());

        await _cache.Received(1)
            .SetAsync(   
                Arg.Any<string>(),
                Arg.Any<byte[]>(),
                Arg.Any<DistributedCacheEntryOptions>(),
                Arg.Any<CancellationToken>());
    }
}