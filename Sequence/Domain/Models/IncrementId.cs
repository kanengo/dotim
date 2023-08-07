using MongoDB.Bson.Serialization.Attributes;

namespace Sequence.Domain.Models;

public class IncrementId
{
    [BsonId]
    public string? BizId { get; set; }
    
    public int MaxId { get; set; }
}