using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace Fennec.Tests.Integration;

// TODO: use fixtures instead
public class MongoDbFactory : IAsyncLifetime
{
    private MongoDbContainer DbContainer { get; }= new MongoDbBuilder().Build();
    private MongoClient Client { get; set; } = null!;
    protected IMongoDatabase Database { get; set; } = null!;

    public async Task InitializeAsync()
    {
        await DbContainer.StartAsync();
        Client = new MongoClient(DbContainer.GetConnectionString());
        Database = Client.GetDatabase("TestDatabase");
    }
    
    public async Task DisposeAsync()
    {
        await DbContainer.DisposeAsync();
    }
}