using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Auth.Domain.Models;

public class Account
{
    [BsonId]
    public int Id { get; set; }
    
    public string? Username { get; set; }
    
    public string? Password { get; set; }
    
    public string? Token { get; set; }
    
    public int Status { get; set; }
    
    // [BsonRepresentation(BsonType.DateTime)]
    public DateTime CreateAt { get; set; }

    public bool IsPasswordCorrect(string pwd) => Password == pwd;
}