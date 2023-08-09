using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Auth.Domain.Models;


namespace Auth.Infrastructure;

public class DatabaseService
{
    public IMongoCollection<Account> AccountCollection { get; }
    
    public DatabaseService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.Connection);

        var database = mongoClient.GetDatabase(databaseSettings.Value.Database);
        
        //集合
        AccountCollection = database.GetCollection<Account>(databaseSettings.Value.AccountCollection);
    }
}