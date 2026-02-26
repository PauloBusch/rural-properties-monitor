using Analitycs.Domain.Entity.Property;

namespace Analitycs.Domain.Interfaces;
public interface IPropertiesApiClient
{
    Task<List<Property>> GetPropertiesByProducerAsync(
        string producerId,
        string token,
        CancellationToken cancellationToken);
}
