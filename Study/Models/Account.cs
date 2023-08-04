using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Study.Models;

public class Account
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("Name")]
    public string Name { get; set; } = null!;

    public DateTime CreateTime { get; set; }
}