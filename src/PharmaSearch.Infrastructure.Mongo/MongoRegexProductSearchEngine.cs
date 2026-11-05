using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using PharmaSearch.Application.Abstractions;
using PharmaSearch.Application.Search;

namespace PharmaSearch.Infrastructure.Mongo;

/// <summary>
/// O "LIKE '%termo%'" no MongoDB: cada palavra precisa aparecer em searchText
/// (campo já sem acento). Acha prefixo e pedaço de palavra, mas um regex sem âncora
/// não usa índice de forma eficiente e não tem nota de relevância.
/// </summary>
internal sealed class MongoRegexProductSearchEngine(MongoCatalogContext context) : IProductSearchEngine
{
    public SearchEngineKind Kind => SearchEngineKind.MongoRegex;

    public async Task<IReadOnlyList<ProductHit>> SearchAsync(ProductSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var filters = Builders<MongoProductDocument>.Filter;

        var clauses = criteria.Term.Tokens
            .Select(token => filters.Regex(x => x.SearchText, new BsonRegularExpression(Regex.Escape(token))))
            .ToList();

        if (criteria.Category is not null)
            clauses.Add(filters.Eq(x => x.Category, criteria.Category));

        var projection = Builders<MongoProductDocument>.Projection
            .Include(x => x.Name)
            .Include(x => x.ActiveIngredient)
            .Include(x => x.Brand)
            .Include(x => x.Category)
            .Include(x => x.Price);

        var rows = await context.Products
            .Find(filters.And(clauses))
            .Project<MongoSearchRow>(projection)
            .Limit(criteria.Size)
            .ToListAsync(cancellationToken);

        return rows.Select(MongoMapping.ToHit).ToList();
    }
}
