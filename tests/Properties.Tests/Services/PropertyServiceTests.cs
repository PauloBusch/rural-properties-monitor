using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Properties.Models;
using Properties.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Properties.Tests.Services
{
    public class PropertyServiceTests
    {
        private readonly Mock<IMongoCollection<Property>> _collectionMock;
        private readonly PropertyService _service;

        public PropertyServiceTests()
        {
            _collectionMock = new Mock<IMongoCollection<Property>>();

            var dbMock = new Mock<IMongoDatabase>();
            dbMock.Setup(d => d.GetCollection<Property>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
                  .Returns(_collectionMock.Object);

            _service = new PropertyService(dbMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldCallInsertOneOnce()
        {
            var newProperty = new Property { Name = "Fazenda Teste 2", Location = "RS", ProducerId = "prod-1" };

            await _service.CreateAsync(newProperty);
            _collectionMock.Verify(c => c.InsertOneAsync(
                It.Is<Property>(p => p.Name == "Fazenda Teste 2"),
                null,
                default), Times.Once);
        }

        [Fact]
        public async Task AddPlotAsync_ShouldCallUpdateOne_WhenValid()
        {
            var propertyId = "prop-123";
            var plot = new Plot { Name = "Talhão Norte", AreaHectares = 50, CropType = "Soja" };

            await _service.AddPlotAsync(propertyId, plot);

            _collectionMock.Verify(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<Property>>(),
                It.IsAny<UpdateDefinition<Property>>(),
                null,
                default), Times.Once);
        }

        [Fact]
        public async Task GetByProducerAsync_ShouldBeCalledWithCorrectFilter()
        {
            await _service.GetByProducerAsync("producer-abc");

            _collectionMock.Verify(c => c.FindAsync(
                It.IsAny<FilterDefinition<Property>>(),
                It.IsAny<FindOptions<Property, Property>>(),
                default), Times.AtLeastOnce);
        }
    }
}