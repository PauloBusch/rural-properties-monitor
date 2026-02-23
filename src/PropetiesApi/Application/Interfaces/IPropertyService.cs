using PropertiesService.Application.DTOs;
using PropertiesService.Domain;

namespace PropetiesApi.Application.Interfaces
{
    public interface IPropertyService
    {
        Task<List<Property>> GetAllAsync();
        Task<Property?> GetByIdAsync(string id);
        Task<Property> CreateAsync(CreatePropertyRequest request);
        Task<Property?> UpdateAsync(string id, UpdatePropertyRequest request);
        Task<bool> DeleteAsync(string id);
    }
}
