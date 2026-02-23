using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Properties.Models;
using Properties.Settings;

namespace Properties.Services;

public class PropertyService
{
    private readonly IMongoCollection<Property> _properties;

    public PropertyService(IOptions<MongoDbSettings> mongoSettings)
    {
        var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
        _properties = mongoDatabase.GetCollection<Property>("Properties");
    }

    // Listar todas as propriedades de um produtor específico
    public async Task<List<Property>> GetAsync(string producerId) =>
        await _properties.Find(x => x.ProducerId == producerId).ToListAsync();

    // Obter uma propriedade específica por ID
    public async Task<Property?> GetByIdAsync(string id) =>
        await _properties.Find(x => x.Id == id).FirstOrDefaultAsync();

    // Criar nova propriedade
    public async Task CreateAsync(Property newProperty) =>
        await _properties.InsertOneAsync(newProperty);

    // Adicionar um talhão a uma propriedade existente (Requisito Funcional)
    public async Task AddPlotAsync(string propertyId, Plot plot)
    {
        var filter = Builders<Property>.Filter.Eq(x => x.Id, propertyId);
        var update = Builders<Property>.Update.Push(x => x.Plots, plot);
        await _properties.UpdateOneAsync(filter, update);
    }
}