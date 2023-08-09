using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

using Sequence.Domain.Models;
namespace Sequence.Infrastructure;

public class IncrementIdService
{   
    
    private readonly DatabaseService _databaseService;

    public IncrementIdService(DatabaseService databaseService)
    {
        // _incrementIdCollection = databaseService.MongoDatabase.GetCollection<IncrementId>("IncrementId");
        _databaseService = databaseService;
    }

    public async Task<long> GetIncrementId(string bizId)
    {
        var updates = Builders<IncrementId>.Update.Inc("MaxId", 1);
        var options = new FindOneAndUpdateOptions<IncrementId,BsonDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After,
        };
        var updateDoc = await _databaseService.IncrementIdCollection.FindOneAndUpdateAsync(
            x => x.BizId == bizId, updates, options);
        
        return updateDoc["MaxId"].ToInt64();
    }
}