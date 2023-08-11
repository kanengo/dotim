using Auth.Domain.Models;
using Auth.Infrastructure.Config;
using MongoDB.Driver;

namespace Auth.Infrastructure.Data;

public class DatabaseService
{
    public IMongoCollection<Account> AccountCollection { get; }
    
    public DatabaseService(DatabaseSettings databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Connection);

        var database = mongoClient.GetDatabase(databaseSettings.DatabaseName);
        
        //集合
        AccountCollection = database.GetCollection<Account>(databaseSettings.AccountCollection);
    }
}