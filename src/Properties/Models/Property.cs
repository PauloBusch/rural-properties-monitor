using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Properties.Models;

public class Property
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("producerId")]
    public string ProducerId { get; set; } = null!;

    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("location")]
    public string Location { get; set; } = null!;

    [BsonElement("plots")]
    public List<Plot> Plots { get; set; } = new();
}

public class Plot
{
    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("cropType")]
    public string CropType { get; set; } = null!;

    [BsonElement("areaHectares")]
    public double AreaHectares { get; set; }
}