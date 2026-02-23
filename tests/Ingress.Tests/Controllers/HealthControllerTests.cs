using FluentAssertions;
using IngressApi.Controller;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ingress.Tests.Controllers;

/// <summary>
/// Os testes para o HealthController verificam se o endpoint de status de saúde retorna as informações corretas sobre o ambiente e o serviço, garantindo que a API esteja funcionando conforme esperado. Eles validam tanto o código de status HTTP quanto os dados retornados, como o nome do serviço e do ambiente.
/// </summary>
public class HealthControllerTests
{
    [Fact]
    public void GetStatus_ReturnsOkResultWithHealthyStatus()
    {
        // Arrange
        var envMock = new Mock<IHostEnvironment>();
        envMock.Setup(e => e.ApplicationName).Returns("IngressApi");
        envMock.Setup(e => e.EnvironmentName).Returns("Test");

        var controller = new HealthController(envMock.Object);

        // Action
        var result = controller.GetStatus() as OkObjectResult; 

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void GetStatus_ReturnsCorrectEnvironmentName()
    {
        // Arrange
        var expectedEnvironment = "Development"; // Definido "Development" para o teste
        var envMock = new Mock<IHostEnvironment>();
        envMock.Setup(e => e.ApplicationName).Returns("IngressApi");
        envMock.Setup(e => e.EnvironmentName).Returns(expectedEnvironment);

        var controller = new HealthController(envMock.Object);

        // Action
        var result = controller.GetStatus() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        var value = result!.Value;
        
        var environmentProperty = value!.GetType().GetProperty("environment");
        environmentProperty.Should().NotBeNull();
        environmentProperty!.GetValue(value).Should().Be(expectedEnvironment);
    }

    [Fact]
    public void GetStatus_ReturnsCorrectServiceName()
    {
        // Arrange
        var expectedServiceName = "IngressApi";
        var envMock = new Mock<IHostEnvironment>();
        envMock.Setup(e => e.ApplicationName).Returns(expectedServiceName);
        envMock.Setup(e => e.EnvironmentName).Returns("Test");

        var controller = new HealthController(envMock.Object);

        // Action
        var result = controller.GetStatus() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        var value = result!.Value;
        
        var serviceProperty = value!.GetType().GetProperty("service");
        serviceProperty.Should().NotBeNull();
        serviceProperty!.GetValue(value).Should().Be(expectedServiceName);
    }
}


