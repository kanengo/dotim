using MongoDB.Driver;
using Sequence.Domain.Models;
using Sequence.Infrastructure;
using Xunit.Abstractions;

namespace Sequence.Tests.Infrastructure;

public class IncrementIdServiceShould
{
    private readonly IMongoDatabase _database = new MongoClient("mongodb://root:123456@localhost:27017").GetDatabase("DotIM");
    
    
    [Fact]
    public async Task GetIncrementId()
    {

        var service = new IncrementIdService(_database.GetCollection<IncrementId>("IncrementId"));

        var incrId = await service.GetIncrementId("test");

        Assert.Equal(2, incrId);
    }
}