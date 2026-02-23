using PropertiesService.Application.DTOs;
using PropertiesService.Domain;

namespace PropetiesApi.Application.Interfaces
{
    public interface IPlotService
    {
        Task<List<Plot>> GetAllByPropertyIdAsync(string propertyId);
        Task<Plot?> GetByIdAsync(string propertyId, string plotId);
        Task<Plot?> CreateAsync(string propertyId, CreatePlotRequest request);
        Task<Plot?> UpdateAsync(string propertyId, string plotId, UpdatePlotRequest request);
        Task<bool> DeleteAsync(string propertyId, string plotId);
    }
}
