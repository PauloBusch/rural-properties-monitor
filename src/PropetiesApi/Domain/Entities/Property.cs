using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PropertiesService.Domain;

public class Property
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    [BsonElement("name")]
    public string Name { get; set; } = default!;

    [BsonElement("location")]
    public Location Location { get; set; } = default!;

    [BsonElement("totalAreaHectares")]
    public double TotalAreaHectares { get; set; }

    [BsonElement("plots")]
    public List<Plot> Plots { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}