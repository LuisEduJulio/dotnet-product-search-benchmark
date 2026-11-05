using MongoDB.Bson;
using MongoDB.Driver;
using PharmaSearch.Application.Abstractions;
using PharmaSearch.Domain.Products;

namespace PharmaSearch.Infrastructure.Mongo;

/// <summary>
/// Recria a coleção com os índices e insere o catálogo. Um writer atende as duas
/// estratégias Mongo ($text e $regex), porque é a mesma coleção.
/// </summary>
internal sealed class MongoCatalogWriter(MongoCatalogContext context) : IProductCatalogWriter
{
    public string StoreName => "MongoDB";

    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        await context.Database.DropCollectionAsync(context.Products.CollectionNamespace.CollectionName, cancellationToken);

        var keys = Builders<MongoProductDocument>.IndexKeys;

        // Índice de texto: stemming em português, sem acento (versão 3) e pesos por campo,
        // equivalentes aos boosts usados no Elasticsearch.
        var textIndex = new CreateIndexModel<MongoProductDocument>(
            keys.Text(x => x.Name).Text(x => x.ActiveIngredient).Text(x => x.Brand).Text(x => x.Description),
            new CreateIndexOptions
            {
                Name = "ix_text_search",
                DefaultLanguage = "portuguese",
                Weights = new BsonDocument
                {
                    { "name", 3 },
                    { "activeIngredient", 3 },
                    { "brand", 2 },
                    { "description", 1 }
                }
            });

        var categoryIndex = new CreateIndexModel<MongoProductDocument>(
            keys.Ascending(x => x.Category), new CreateIndexOptions { Name = "ix_category" });

        await context.Products.Indexes.CreateManyAsync(new[] { textIndex, categoryIndex }, cancellationToken);
    }

    public Task IndexAsync(IReadOnlyCollection<Product> products, CancellationToken cancellationToken) =>
        products.Count == 0
            ? Task.CompletedTask
            : context.Products.InsertManyAsync(
                products.Select(MongoProductDocument.From),
                new InsertManyOptions { IsOrdered = false },
                cancellationToken);

    /// <summary>No MongoDB a escrita confirmada já é lida pela próxima consulta.</summary>
    public Task MakeSearchableAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
