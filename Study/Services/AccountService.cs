using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Study.Models;

namespace Study.Services;

public class AccountService
{
    private readonly IMongoCollection<Account> _accountCollection;

    public AccountService(
        BookStoreDatabase bookStoreDatabase,IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
    {

        _accountCollection = bookStoreDatabase.MongoDatabase.GetCollection<Account>(
            bookStoreDatabaseSettings.Value.AccountCollectionName);
    }
    
    public async Task<List<Account>> GetAsync() =>
        await _accountCollection.Find(_ => true).ToListAsync();

    public async Task<Account?> GetAsync(int id) =>
        await _accountCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Account account) =>
        await _accountCollection.InsertOneAsync(account);

    public async Task UpdateAsync(int id, Account updateAccount) =>
        await _accountCollection.ReplaceOneAsync(x => x.Id == id, updateAccount);

    public async Task RemoveAsync(int id) =>
        await _accountCollection.DeleteOneAsync(x => x.Id == id);
}