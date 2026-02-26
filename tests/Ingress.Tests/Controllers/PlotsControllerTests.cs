using FluentAssertions;
using IngressApi.Controller;
using IngressApi.DTO;
using IngressApi.Repositories;
using IngressApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ingress.Tests.Controllers;

public class PlotsControllerTests
{
    private readonly Mock<ISensorDataService> _serviceMock;
    private readonly Mock<ISensorDataRepository> _repositoryMock;
    private readonly PlotsController _controller;

    public PlotsControllerTests()
    {
        _serviceMock = new Mock<ISensorDataService>();
        _repositoryMock = new Mock<ISensorDataRepository>();
        _controller = new PlotsController(_serviceMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task GetSensorData_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var plotIds = "plot-001,plot-002";
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 2);

        // Controller normalizes to UTC; endDate midnight gets AddDays(1)
        var expectedStartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        var expectedEndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc).AddDays(1);

        var expectedResponse = new List<SensorDataResponse>
        {
            new SensorDataResponse { PlotId = "plot-001" },
            new SensorDataResponse { PlotId = "plot-001" }
        };

        _serviceMock.Setup(s => s.GetAggregatedDataAsync(
                It.IsAny<List<string>>(),
                expectedStartDate,
                expectedEndDate,
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
                It.Is<List<string>>(list => list.Contains("plot-001") && list.Contains("plot-002")),
                expectedStartDate,
                expectedEndDate,
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
        badRequest!.Value.Should().Be("plotIds é obrigatório. Exemplo: plot-001,plot-002");
        
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
        var plotIds = "plot-001";
        var startDate = new DateTime(2026, 1, 22);
        var endDate = new DateTime(2026, 1, 1);

        // Act
        var result = await _controller.GetSensorData(plotIds, startDate, endDate, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("startDate deve ser anterior ou igual a endDate");
    }

    [Fact]
    public async Task GetSensorData_WithStartDateEqualsEndDate_ReturnsOkResult()
    {
        // Arrange
        var plotIds = "plot-001";
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 1);

        _serviceMock
            .Setup(s => s.GetAggregatedDataAsync(
                It.IsAny<List<string>>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SensorDataResponse>());

        // Act
        var result = await _controller.GetSensorData(plotIds, startDate, endDate, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetSensorData_WithMultiplePlotIds_ParsesCorrectly()
    {
        // Arrange
        var plotIds = "plot-001, plot-002, plot-003";
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 2);

        // Controller normalizes to UTC; endDate midnight gets AddDays(1)
        var expectedStartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        var expectedEndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc).AddDays(1);

        _serviceMock
            .Setup(s => s.GetAggregatedDataAsync(
                It.IsAny<List<string>>(),
                expectedStartDate,
                expectedEndDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SensorDataResponse>());

        // Act
        await _controller.GetSensorData(plotIds, startDate, endDate, CancellationToken.None);

        // Assert
        _serviceMock.Verify(
            s => s.GetAggregatedDataAsync(
                It.Is<List<string>>(list => list.Count == 3 && 
                                            list.Contains("plot-001") && 
                                            list.Contains("plot-002") && 
                                            list.Contains("plot-003")),
                expectedStartDate,
                expectedEndDate,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSensorData_WithPlotIdsContainingSpaces_TrimsSpaces()
    {
        // Arrange
        var plotIds = " plot-001 , plot-002 , plot-003 ";
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 2);

        // Controller normalizes to UTC; endDate midnight gets AddDays(1)
        var expectedStartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        var expectedEndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc).AddDays(1);

        _serviceMock
            .Setup(s => s.GetAggregatedDataAsync(
                It.IsAny<List<string>>(),
                expectedStartDate,
                expectedEndDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SensorDataResponse>());

        // Act
        await _controller.GetSensorData(plotIds, startDate, endDate, CancellationToken.None);

        // Assert
        _serviceMock.Verify(
            s => s.GetAggregatedDataAsync(
                It.Is<List<string>>(list => 
                    list.All(id => !id.StartsWith(" ") && !id.EndsWith(" "))),
                expectedStartDate,
                expectedEndDate,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

