using FluentAssertions;
using IngressApi.Controller;
using IngressApi.DTO;
using IngressApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ingress.Tests.Controllers;

public class PlotsControllerTests
{
    private readonly Mock<ISensorDataService> _serviceMock;
    private readonly PlotsController _controller;

    public PlotsControllerTests()
    {
        _serviceMock = new Mock<ISensorDataService>();
        _controller = new PlotsController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetSensorData_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var plotIds = "plot1,plot2";
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 2);

        var expectedResponse = new List<SensorDataResponse>
        {
            new SensorDataResponse { PlotId = "plot1" },
            new SensorDataResponse { PlotId = "plot2" }
        };

        _serviceMock.Setup(s => s.GetAggregatedDataAsync(
                It.IsAny<List<string>>(),
                startDate,
                endDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetSensorData(plotIds, startDate, endDate, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(expectedResponse);
        
        _serviceMock.Verify(
            s => s.GetAggregatedDataAsync(
                It.Is<List<string>>(list => list.Contains("plot1") && list.Contains("plot2")),
                startDate,
                endDate,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSensorData_WithEmptyPlotIds_ReturnsBadRequest()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 2);

        // Act
        var result = await _controller.GetSensorData("", startDate, endDate, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("plotIds é obrigatório");
        
        _serviceMock.Verify(
            s => s.GetAggregatedDataAsync(
                It.IsAny<List<string>>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetSensorData_WithWhitespacePlotIds_ReturnsBadRequest()
    {
        // Arrange
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 2);

        // Act
        var result = await _controller.GetSensorData("   ", startDate, endDate, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetSensorData_WithStartDateAfterEndDate_ReturnsBadRequest()
    {
        // Arrange
        var plotIds = "plot1";
        var startDate = new DateTime(2026, 1, 22);
        var endDate = new DateTime(2026, 1, 1);

        // Act
        var result = await _controller.GetSensorData(plotIds, startDate, endDate, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("startDate deve ser anterior a endDate");
    }

    [Fact]
    public async Task GetSensorData_WithStartDateEqualsEndDate_ReturnsBadRequest()
    {
        // Arrange
        var plotIds = "plot1";
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 1);

        // Act
        var result = await _controller.GetSensorData(plotIds, startDate, endDate, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetSensorData_WithMultiplePlotIds_ParsesCorrectly()
    {
        // Arrange
        var plotIds = "plot1,plot2,plot3";
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 2);

        _serviceMock
            .Setup(s => s.GetAggregatedDataAsync(
                It.IsAny<List<string>>(),
                startDate,
                endDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SensorDataResponse>());

        // Act
        await _controller.GetSensorData(plotIds, startDate, endDate, CancellationToken.None);

        // Assert
        _serviceMock.Verify(
            s => s.GetAggregatedDataAsync(
                It.Is<List<string>>(list => list.Count == 3 && 
                                            list.Contains("plot1") && 
                                            list.Contains("plot2") && 
                                            list.Contains("plot3")),
                startDate,
                endDate,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSensorData_WithPlotIdsContainingSpaces_TrimsSpaces()
    {
        // Arrange
        var plotIds = " plot1 , plot2 , plot3 ";
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 2);

        _serviceMock
            .Setup(s => s.GetAggregatedDataAsync(
                It.IsAny<List<string>>(),
                startDate,
                endDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SensorDataResponse>());

        // Act
        await _controller.GetSensorData(plotIds, startDate, endDate, CancellationToken.None);

        // Assert
        _serviceMock.Verify(
            s => s.GetAggregatedDataAsync(
                It.Is<List<string>>(list => 
                    list.All(id => !id.StartsWith(" ") && !id.EndsWith(" "))),
                startDate,
                endDate,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

