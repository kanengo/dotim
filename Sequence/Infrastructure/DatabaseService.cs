using Microsoft.Extensions.Options;
using MongoDB.Driver;


namespace Sequence.Infrastructure;

public class DatabaseService
{
    public IMongoDatabase MongoDatabase { get; }

    public DatabaseService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.Connection);

        MongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.Database);
        
    }
}