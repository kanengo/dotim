using MongoDB.Bson.Serialization.Attributes;

namespace Sequence.Domain.Models;

public class IncrementId
{
    [BsonId]
    public string? BizId { get; set; }
    
    public long MaxId { get; set; }
}