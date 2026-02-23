using MongoDB.Driver;
using PropertiesService.Application.DTOs;
using PropertiesService.Domain;
using PropertiesService.Infrastructure.Mongo;
using Domain.Entities;
using PropetiesApi.Application.Interfaces;

namespace PropetiesApi.Application.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly MongoContext _context;

        public PropertyService(MongoContext context)
        {
            _context = context;
        }

        public async Task<List<Property>> GetAllAsync()
        {
            return await _context.Properties.Find(_ => true).ToListAsync();
        }

        public async Task<Property?> GetByIdAsync(string id)
        {
            return await _context.Properties.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Property> CreateAsync(CreatePropertyRequest request)
        {
            var property = new Property
            {
                Name = request.Name,
                Location = new Location
                {
                    City = request.City,
                    State = request.State,
                    Country = "BR"
                },
                TotalAreaHectares = request.TotalAreaHectares,
                Plots = new List<Plot>(),
                CreatedAt = DateTime.UtcNow
            };

            await _context.Properties.InsertOneAsync(property);
            return property;
        }

        public async Task<Property?> UpdateAsync(string id, UpdatePropertyRequest request)
        {
            var property = await GetByIdAsync(id);

            if (property == null)
            {
                return null;
            }

            property.Name = request.Name;
            property.Location.City = request.City;
            property.Location.State = request.State;
            property.TotalAreaHectares = request.TotalAreaHectares;

            await _context.Properties.ReplaceOneAsync(p => p.Id == id, property);
            return property;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _context.Properties.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
