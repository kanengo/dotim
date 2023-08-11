using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Sequence.Domain.Models;
using Sequence.Infrastructure.Config;

namespace Sequence.Infrastructure.Data;

public class DatabaseService
{
    public IMongoCollection<IncrementId> IncrementIdCollection { get; }
    
    public DatabaseService(DatabaseSettings databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Connection);

        var database = mongoClient.GetDatabase(databaseSettings.DatabaseName);
        
        //集合
        IncrementIdCollection = database.GetCollection<IncrementId>(databaseSettings.IncrementIdCollection);
    }
}