using Auth.Domain.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;


namespace Auth.Infrastructure;

public class AccountService
{   
    
    private readonly DatabaseService _databaseService;

    public AccountService(DatabaseService databaseService)
    {
        // _incrementIdCollection = databaseService.MongoDatabase.GetCollection<IncrementId>("IncrementId");
        _databaseService = databaseService;
    }


    public async Task<Account?> GetByUsernameAsync(string username)
    {
        return await _databaseService.AccountCollection.Find(x => x.Username == username).FirstOrDefaultAsync();
    }
    
    public async Task CreateAsync(Account account) =>
        await _databaseService.AccountCollection.InsertOneAsync(account);
}