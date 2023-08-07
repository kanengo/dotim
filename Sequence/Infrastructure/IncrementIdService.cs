using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

using Sequence.Domain.Models;
namespace Sequence.Infrastructure;

public class IncrementIdService
{   
    
    private readonly IMongoCollection<IncrementId> _incrementIdCollection;

    public IncrementIdService(DatabaseService databaseService,IOptions<DatabaseSettings> databaseSettings)
    {
        _incrementIdCollection = databaseService.MongoDatabase.GetCollection<IncrementId>(databaseSettings.Value.IncrementIdCollection);
    }

    public async Task<int> GetIncrementId(string bizId)
    {
        var updates = Builders<BsonDocument>.Update.Inc("MaxId", 1);
         _incrementIdCollection.FindOneAndUpdateAsync(x => x.BizId == bizId,updates);
    }
}