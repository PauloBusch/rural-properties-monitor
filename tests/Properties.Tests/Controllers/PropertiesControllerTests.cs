using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Properties.Controller;
using Properties.DTO;
using Properties.Models;
using Properties.Services;


namespace Properties.Tests.Controllers
{
    public class PropertiesControllerTests
    {
        private readonly Mock<IPropertyService> _serviceMock;
        private readonly PropertiesController _controller;

        public PropertiesControllerTests()
        {
            _serviceMock = new Mock<IPropertyService>();
            _controller = new PropertiesController(_serviceMock.Object);
        }

        [Fact]
        public async Task Create_ShouldReturnCreatedAtAction_WhenValidDto()
        {
            var dto = new PropertyCreateDto
            {
                Name = "Fazenda Teste 01",
                Location = "São Paulo, SP",
                ProducerId = "prod-123"
            };

            var result = await _controller.Create(dto);

            var actionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            actionResult.StatusCode.Should().Be(201);

            _serviceMock.Verify(s => s.CreateAsync(It.Is<Property>(p =>
                p.Name == dto.Name &&
                p.ProducerId == dto.ProducerId)), Times.Once);
        }

        [Fact]
        public async Task AddPlot_ShouldReturnOk_WhenPlotIsAddedSuccessfully()
        {
            var propertyId = "prop-123";
            var plotDto = new PlotCreateDto { Name = "Talhão 01", CropType = "Soja", AreaHectares = 50 };

            _serviceMock.Setup(s => s.GetByIdAsync(propertyId))
                        .ReturnsAsync(new Property { Id = propertyId, Name = "Fazenda Existente" });

            var result = await _controller.AddPlot(propertyId, plotDto);

            result.Should().BeOfType<OkObjectResult>();
            _serviceMock.Verify(s => s.AddPlotAsync(propertyId, It.IsAny<Plot>()), Times.Once);
        }

        [Fact]
        public async Task AddPlot_ShouldReturnNotFound_WhenPropertyDoesNotExist()
        {
            var propertyId = "id-inexistente";
            var plotDto = new PlotCreateDto { Name = "Talhão 01", CropType = "Soja", AreaHectares = 50 };

            _serviceMock.Setup(s => s.GetByIdAsync(propertyId))
                        .ReturnsAsync((Property?)null);

            var result = await _controller.AddPlot(propertyId, plotDto);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetByProducer_ShouldReturnOk_WithListOfPropertiesMappedToDto()
        {
            var producerId = "prod-123";
            var mockList = new List<Property>
            {
                new Property {
                    Id = "1",
                    Name = "Fazenda Teste 01",
                    Location = "Local A",
                    ProducerId = producerId,
                    Plots = new List<Plot> { new Plot { Name = "Talhão 01", AreaHectares = 50, CropType = "Milho" } }
                },
                new Property {
                    Id = "2",
                    Name = "Fazenda Teste 02",
                    Location = "Local B",
                    ProducerId = producerId,
                    Plots = new List<Plot>()
                }
            };

            _serviceMock.Setup(s => s.GetByProducerAsync(producerId))
                        .ReturnsAsync(mockList);

            var actionResult = await _controller.GetByProducer(producerId);

            var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedProperties = okResult.Value.Should().BeAssignableTo<IEnumerable<PropertyResponseDto>>().Subject.ToList();

            returnedProperties.Should().HaveCount(2);
            returnedProperties[0].Name.Should().Be("Fazenda Teste 01");
            returnedProperties[0].Plots.Should().NotBeEmpty();
            returnedProperties[0].Plots.First().Name.Should().Be("Talhão 01");
            returnedProperties[1].Plots.Should().BeEmpty();
        }
    }
}