using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Properties.Models;
using Properties.Settings; 

namespace Properties.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly IMongoCollection<Property> _propertiesCollection;

        public PropertyService(IMongoDatabase database)
        {
            _propertiesCollection = database.GetCollection<Property>("Properties");
        }

        public async Task CreateAsync(Property property)
        {
            await _propertiesCollection.InsertOneAsync(property);
        }

        public async Task<IEnumerable<Property>> GetByProducerAsync(string producerId)
        {
            return await _propertiesCollection.Find(p => p.ProducerId == producerId).ToListAsync();
        }

        public async Task<Property?> GetByIdAsync(string id)
        {
            return await _propertiesCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddPlotAsync(string propertyId, Plot plot)
        {
            var filter = Builders<Property>.Filter.Eq(p => p.Id, propertyId);
            var update = Builders<Property>.Update.Push(p => p.Plots, plot);

            await _propertiesCollection.UpdateOneAsync(filter, update);
        }
    }
}