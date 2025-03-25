using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class MotorCatalog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }
    public required string URL { get; set; }
    public required string Technology { get; set; }
    public required string Power { get; set; }
    public required string Model { get; set; }
    public required string FrameSize { get; set; }
    public required string Poles { get; set; }
    public required string Standard { get; set; }
    public required string Voltage { get; set; }
    public required string MountingType { get; set; }
    public required string Material { get; set; }
    public required string Protection { get; set; }
    public required string ShaftDiameter { get; set; }
    public required string Footprint { get; set; }
}
