using FluentAssertions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Properties.Models;
using Properties.Services;
using Properties.Settings;

namespace Properties.Tests.Services;

public class PropertyServiceTests
{
    private readonly Mock<IMongoCollection<Property>> _collectionMock;
    private readonly PropertyService _service;

    public PropertyServiceTests()
    {
        _collectionMock = new Mock<IMongoCollection<Property>>();

        var dbMock = new Mock<IMongoDatabase>();
        dbMock.Setup(d => d.GetCollection<Property>(It.IsAny<string>(), null))
              .Returns(_collectionMock.Object);

        _service = new PropertyService(dbMock.Object);
    }

    [Fact]
    public async Task AddPlotAsync_ShouldThrowException_WhenAreaIsNegative()
    {
        var propertyId = "prop-123";
        var invalidPlot = new Plot { Name = "Invalid Plot", AreaHectares = -10 };

        var action = () => _service.AddPlotAsync(propertyId, invalidPlot);

        await action.Should().ThrowAsync<ArgumentException>()
              .WithMessage("The area of ​​the plot must be greater than zero");
    }

    [Fact]
    public async Task CreateAsync_ShouldSetId_WhenSuccessful()
    {
        var newProperty = new Property { Name = "Fazenda Teste 2", Location = "RS" };

        await _service.CreateAsync(newProperty);

        _collectionMock.Verify(c => c.InsertOneAsync(
            It.IsAny<Property>(),
            null,
            default), Times.Once);
    }
}