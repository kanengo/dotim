using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Sequence.Domain.Models;


namespace Sequence.Infrastructure;

public class DatabaseService
{
    public IMongoCollection<IncrementId> IncrementIdCollection { get; }
    
    public DatabaseService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.Connection);

        var database = mongoClient.GetDatabase(databaseSettings.Value.Database);
        
        //集合
        IncrementIdCollection = database.GetCollection<IncrementId>(databaseSettings.Value.IncrementIdCollection);
    }
}