using MongoDB.Driver;
using PropertiesService.Application.DTOs;
using PropertiesService.Domain;
using PropertiesService.Infrastructure.Mongo;
using PropetiesApi.Application.Interfaces;

namespace PropetiesApi.Application.Services
{
    public class PlotService : IPlotService
    {
        private readonly MongoContext _context;

        public PlotService(MongoContext context)
        {
            _context = context;
        }

        public async Task<List<Plot>> GetAllByPropertyIdAsync(string propertyId)
        {
            var property = await _context.Properties
                .Find(p => p.Id == propertyId)
                .FirstOrDefaultAsync();

            return property?.Plots ?? new List<Plot>();
        }

        public async Task<Plot?> GetByIdAsync(string propertyId, string plotId)
        {
            var property = await _context.Properties
                .Find(p => p.Id == propertyId)
                .FirstOrDefaultAsync();

            return property?.Plots.FirstOrDefault(plot => plot.Id == plotId);
        }

        public async Task<Plot?> CreateAsync(string propertyId, CreatePlotRequest request)
        {
            var property = await _context.Properties
                .Find(p => p.Id == propertyId)
                .FirstOrDefaultAsync();

            if (property == null)
            {
                return null;
            }

            var plot = new Plot
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Crop = request.Crop,
                AreaHectares = request.AreaHectares,
                Coordinates = new Coordinates
                {
                    Latitude = request.Latitude,
                    Longitude = request.Longitude
                },
                CreatedAt = DateTime.UtcNow
            };

            property.Plots.Add(plot);

            await _context.Properties.ReplaceOneAsync(p => p.Id == propertyId, property);

            return plot;
        }

        public async Task<Plot?> UpdateAsync(string propertyId, string plotId, UpdatePlotRequest request)
        {
            var property = await _context.Properties
                .Find(p => p.Id == propertyId)
                .FirstOrDefaultAsync();

            if (property == null)
            {
                return null;
            }

            var plot = property.Plots.FirstOrDefault(p => p.Id == plotId);

            if (plot == null)
            {
                return null;
            }

            plot.Name = request.Name;
            plot.Crop = request.Crop;
            plot.AreaHectares = request.AreaHectares;
            plot.Coordinates.Latitude = request.Latitude;
            plot.Coordinates.Longitude = request.Longitude;

            await _context.Properties.ReplaceOneAsync(p => p.Id == propertyId, property);

            return plot;
        }

        public async Task<bool> DeleteAsync(string propertyId, string plotId)
        {
            var property = await _context.Properties
                .Find(p => p.Id == propertyId)
                .FirstOrDefaultAsync();

            if (property == null)
            {
                return false;
            }

            var plot = property.Plots.FirstOrDefault(p => p.Id == plotId);

            if (plot == null)
            {
                return false;
            }

            property.Plots.Remove(plot);

            await _context.Properties.ReplaceOneAsync(p => p.Id == propertyId, property);

            return true;
        }
    }
}
