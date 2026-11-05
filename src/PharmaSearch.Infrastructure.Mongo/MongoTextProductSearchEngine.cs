using MongoDB.Driver;
using PharmaSearch.Application.Abstractions;
using PharmaSearch.Application.Search;

namespace PharmaSearch.Infrastructure.Mongo;

/// <summary>
/// Busca com o índice de texto do MongoDB ($text), ordenada pela textScore.
/// Comportamentos que o benchmark deixa visíveis:
/// - vários termos viram OU ("dipirona gotas" traz qualquer um dos dois);
/// - não tem tolerância a erro de digitação nem busca por prefixo.
/// </summary>
internal sealed class MongoTextProductSearchEngine(MongoCatalogContext context) : IProductSearchEngine
{
    public SearchEngineKind Kind => SearchEngineKind.MongoText;

    public async Task<IReadOnlyList<ProductHit>> SearchAsync(ProductSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var filters = Builders<MongoProductDocument>.Filter;
        var filter = filters.Text(criteria.Term.Raw);
        if (criteria.Category is not null)
            filter = filters.And(filter, filters.Eq(x => x.Category, criteria.Category));

        var projection = Builders<MongoProductDocument>.Projection
            .Include(x => x.Name)
            .Include(x => x.ActiveIngredient)
            .Include(x => x.Brand)
            .Include(x => x.Category)
            .Include(x => x.Price)
            .MetaTextScore("score");

        var rows = await context.Products
            .Find(filter)
            .Project<MongoSearchRow>(projection)
            .Sort(Builders<MongoProductDocument>.Sort.MetaTextScore("score"))
            .Limit(criteria.Size)
            .ToListAsync(cancellationToken);

        return rows.Select(MongoMapping.ToHit).ToList();
    }
}
