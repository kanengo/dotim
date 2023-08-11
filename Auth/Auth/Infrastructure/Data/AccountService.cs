using Auth.Domain.Models;
using MongoDB.Driver;

namespace Auth.Infrastructure.Data;

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

    public async Task UpdateTokenAsync(long id, string token)
    {
        var updates = Builders<Account>.Update.Set("Token", token);
        await _databaseService.AccountCollection.UpdateOneAsync(x => x.Id == id, updates);
    }
    
}