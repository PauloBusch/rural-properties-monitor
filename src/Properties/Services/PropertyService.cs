using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Properties.Models;
using Properties.Settings;

namespace Properties.Services;

public class PropertyService : IPropertyService
{
    private readonly IMongoCollection<Property> _properties;

    public PropertyService(IMongoDatabase database)
    {
        _properties = database.GetCollection<Property>("Properties");
    }

    public async Task<Property?> GetByIdAsync(string id) =>
        await _properties.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Property newProperty) =>
        await _properties.InsertOneAsync(newProperty);

    public async Task AddPlotAsync(string propertyId, Plot plot)
    {
        if (plot.AreaHectares <= 0)
            throw new ArgumentException("The area of ​​the plot must be greater than zero");

        var filter = Builders<Property>.Filter.Eq(p => p.Id, propertyId);
        var update = Builders<Property>.Update.Push(p => p.Plots, plot);
        await _properties.UpdateOneAsync(filter, update);
    }

    public Task<IEnumerable<Property>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Property>> GetByProducerAsync(string producerId)
    {
        throw new NotImplementedException();
    }
}