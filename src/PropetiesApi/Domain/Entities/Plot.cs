using MongoDB.Bson.Serialization.Attributes;

namespace PropertiesService.Domain;

public class Plot
{
    [BsonElement("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [BsonElement("name")]
    public string Name { get; set; } = default!;

    [BsonElement("crop")]
    public string Crop { get; set; } = default!; // milho, soja, etc.

    [BsonElement("areaHectares")]
    public double AreaHectares { get; set; }

    [BsonElement("coordinates")]
    public Coordinates Coordinates { get; set; } = default!;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}