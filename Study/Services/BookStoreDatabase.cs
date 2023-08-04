using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Study.Models;

namespace Study.Services;

public class BookStoreDatabase
{
    public IMongoDatabase MongoDatabase { get; }

    public BookStoreDatabase(
        IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            bookStoreDatabaseSettings.Value.ConnectionString);

        MongoDatabase = mongoClient.GetDatabase(
            bookStoreDatabaseSettings.Value.DatabaseName);
        
    }
}