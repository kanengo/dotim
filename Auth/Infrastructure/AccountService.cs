using MongoDB.Driver;

using Auth.Domain.Models;
using Microsoft.Extensions.Options;

namespace Auth.Infrastructure;

public class AccountService
{
    private readonly IMongoCollection<Account> _accountCollection;

    public AccountService(DatabaseService databaseService,IOptions<DatabaseSettings> databaseSettings)
    {
        _accountCollection = databaseService.MongoDatabase.GetCollection<Account>(databaseSettings.Value.AccountCollection);
    }

    public async Task<Account?> GetAsync(int id) =>
        await _accountCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<Account?> GetByUsernameAsync(string username) =>
        await _accountCollection.Find(x => x.Username == username).FirstOrDefaultAsync();

}