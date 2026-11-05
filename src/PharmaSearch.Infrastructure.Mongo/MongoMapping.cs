using PharmaSearch.Application.Search;

namespace PharmaSearch.Infrastructure.Mongo;

internal static class MongoMapping
{
    public static ProductHit ToHit(MongoSearchRow row) => new(
        row.Sku, row.Name, row.ActiveIngredient, row.Brand, row.Category, row.Price, Math.Round(row.Score, 4));
}
