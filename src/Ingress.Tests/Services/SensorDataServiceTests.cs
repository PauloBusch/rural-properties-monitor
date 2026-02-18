using FluentAssertions;
using IngressApi.DTO;
using IngressApi.Repositories;
using IngressApi.Services;
using Moq;
using Sensors.Models;
using Xunit;

namespace Ingress.Tests.Services;

public class SensorDataServiceTests
{
    private readonly Mock<ISensorDataRepository> _repositoryMock;
    private readonly SensorDataService _service;

    public SensorDataServiceTests()
    {
        _repositoryMock = new Mock<ISensorDataRepository>();
        _service = new SensorDataService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAggregatedDataAsync_WithValidData_ReturnsAggregatedResponse()
    {
        // Arrange
        var plotIds = new List<string> { "plot1", "plot2" };
        var startDate = new DateTime(2026, 1, 1, 0, 0, 0);
        var endDate = new DateTime(2026, 1, 2, 0, 0, 0);

        var sampleData = new List<SensorDataPayload>
        {
            new()
            {
                PlotId = "plot1",
                SoilMoisture = 50.0,
                Temperature = 25.0,
                Precipitation = 10.0,
                Timestamp = new DateTime(2026, 1, 1, 10, 0, 0)
            },
            new()
            {
                PlotId = "plot1",
                SoilMoisture = 52.0,
                Temperature = 26.0,
                Precipitation = 12.0,
                Timestamp = new DateTime(2026, 1, 1, 10, 30, 0)
            },
            new()
            {
                PlotId = "plot2",
                SoilMoisture = 60.0,
                Temperature = 22.0,
                Precipitation = 5.0,
                Timestamp = new DateTime(2026, 1, 1, 10, 0, 0)
            }
        };

        _repositoryMock
            .Setup(r => r.GetByPlotIdsAsync(plotIds, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sampleData);

        // Act
        var result = await _service.GetAggregatedDataAsync(plotIds, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var plot1Data = result.FirstOrDefault(r => r.PlotId == "plot1");
        plot1Data.Should().NotBeNull();
        plot1Data!.HourlyAverages.Should().HaveCount(1);
        plot1Data.PeriodAverage.Should().NotBeNull();

        _repositoryMock.Verify(
            r => r.GetByPlotIdsAsync(plotIds, startDate, endDate, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task GetAggregatedDataAsync_WithEmptyData_ReturnsEmptyList()
    {
        // Arrange
        var plotIds = new List<string> { "plot1" };
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 2);

        _repositoryMock
            .Setup(r => r.GetByPlotIdsAsync(plotIds, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SensorDataPayload>());

        // Act
        var result = await _service.GetAggregatedDataAsync(plotIds, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAggregatedDataAsync_CalculatesHourlyAveragesCorrectly()
    {
        // Arrange
        var plotIds = new List<string> { "plot1" };
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 2);

        var sampleData = new List<SensorDataPayload>
        {
            new()
            {
                PlotId = "plot1",
                SoilMoisture = 50.0,
                Temperature = 20.0,
                Precipitation = 10.0,
                Timestamp = new DateTime(2026, 1, 1, 10, 0, 0)
            },
            new()
            {
                PlotId = "plot1",
                SoilMoisture = 60.0,
                Temperature = 30.0,
                Precipitation = 20.0,
                Timestamp = new DateTime(2026, 1, 1, 10, 30, 0)
            }
        };

        _repositoryMock
            .Setup(r => r.GetByPlotIdsAsync(plotIds, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sampleData);

        // Act
        var result = await _service.GetAggregatedDataAsync(plotIds, startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
        var plotData = result.First();
        plotData.HourlyAverages.Should().HaveCount(1);
        
        var hourlyAvg = plotData.HourlyAverages.First();
        hourlyAvg.SoilMoisture.Should().Be(55.0);
        hourlyAvg.Temperature.Should().Be(25.0);
        hourlyAvg.Precipitation.Should().Be(15.0);
    }

    [Fact]
    public async Task GetAggregatedDataAsync_CalculatesPeriodAverageCorrectly()
    {
        // Arrange
        var plotIds = new List<string> { "plot1" };
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 3);

        var sampleData = new List<SensorDataPayload>
        {
            new SensorDataPayload
            {
                PlotId = "plot1",
                SoilMoisture = 40.0,
                Temperature = 20.0,
                Precipitation = 5.0,
                Timestamp = new DateTime(2026, 1, 1, 10, 0, 0)
            },
            new SensorDataPayload
            {
                PlotId = "plot1",
                SoilMoisture = 60.0,
                Temperature = 30.0,
                Precipitation = 15.0,
                Timestamp = new DateTime(2026, 1, 2, 14, 0, 0)
            }
        };

        _repositoryMock
            .Setup(r => r.GetByPlotIdsAsync(plotIds, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sampleData);

        // Act
        var result = await _service.GetAggregatedDataAsync(plotIds, startDate, endDate);

        // Assert
        var plotData = result.First();
        plotData.PeriodAverage.SoilMoisture.Should().Be(50.0);
        plotData.PeriodAverage.Temperature.Should().Be(25.0);
        plotData.PeriodAverage.Precipitation.Should().Be(10.0);
        plotData.PeriodAverage.StartDate.Should().Be(startDate);
        plotData.PeriodAverage.EndDate.Should().Be(endDate);
    }
}

