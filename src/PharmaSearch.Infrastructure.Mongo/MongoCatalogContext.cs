using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace PharmaSearch.Infrastructure.Mongo;

/// <summary>Um único MongoClient por aplicação (ele já gerencia o pool de conexões).</summary>
internal sealed class MongoCatalogContext
{
    public IMongoDatabase Database { get; }
    public IMongoCollection<MongoProductDocument> Products { get; }

    public MongoCatalogContext(IOptions<MongoOptions> options)
    {
        var settings = MongoClientSettings.FromConnectionString(options.Value.ConnectionString);
        settings.MaxConnectionPoolSize = options.Value.MaxConnectionPoolSize;

        var client = new MongoClient(settings);
        Database = client.GetDatabase(options.Value.Database);
        Products = Database.GetCollection<MongoProductDocument>(options.Value.Collection);
    }
}
