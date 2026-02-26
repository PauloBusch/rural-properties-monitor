using Properties.Models;

namespace Properties.Services;

public interface IPropertyService
{
    Task<Property?> GetByIdAsync(string id);
    Task<IEnumerable<Property>> GetByProducerAsync(string producerId);
    Task CreateAsync(Property property);
    Task AddPlotAsync(string propertyId, Plot plot);
}