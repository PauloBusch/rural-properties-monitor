using Analitycs.Domain.Interfaces;
using Analytics.Tests._Common;
using Analytics.Tests.Factories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace Analytics.Tests.Services;

public class DashboardServiceTests : UseCaseTestBase<DashboardService>
{
    private readonly IPropertiesApiClient _propertiesApiClient;
    private readonly IIngressApiClient _ingressApiClient;

    public DashboardServiceTests(AnalyticsFixture fixture) : base(fixture)
    {
        _propertiesApiClient = GetMock<IPropertiesApiClient>();
        _ingressApiClient = GetMock<IIngressApiClient>();        
    }

    [Fact]
    public async Task ShouldReturnConsolidatedDashboardAsync()
    {
        // Arrange
        var factory = new ModelFactory();

        _propertiesApiClient
            .GetPropertiesByProducerAsync(
                factory.ProducerId,
                factory.Token,
                Arg.Any<CancellationToken>())
            .Returns(factory.Properties);

        _ingressApiClient
            .GetSensorDataAsync(
                Arg.Is<List<string>>(x => x.SequenceEqual(factory.PlotIds.Take(1))), 
                Arg.Is<DateTime>(x => x == factory.StartDate),
                Arg.Is<DateTime>(x => x == factory.EndDate),
                Arg.Any<CancellationToken>())
            .Returns(factory.SensorDataList);

        // Act
        var result = await UseCase.GetDashboardAsync(
            factory.ProducerId,
            factory.Token,
            factory.StartDate,
            factory.EndDate,
            CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.ProducerId.ShouldBe(factory.ProducerId);
        result.Properties.Count.ShouldBe(1);

        var property = result.Properties.First();
        property.Plots.Count.ShouldBe(1);

        var plot = property.Plots.First();
        plot.PlotId.ShouldBe("plot-1");
        plot.PeriodAverage.ShouldNotBeNull();
        plot.PeriodAverage!.SoilMoisture.ShouldBe(35.75);

        await _propertiesApiClient.Received(1)
            .GetPropertiesByProducerAsync(
                factory.ProducerId,
                factory.Token,
                Arg.Any<CancellationToken>());

        await _ingressApiClient.Received(1)
            .GetSensorDataAsync(
                Arg.Any<List<string>>(),
                factory.StartDate,
                factory.EndDate,
                Arg.Any<CancellationToken>());
    }
}