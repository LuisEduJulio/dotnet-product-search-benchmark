using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PharmaSearch.Domain.Products;

namespace PharmaSearch.Infrastructure.Mongo;

/// <summary>Modelo de persistência do MongoDB. O SKU é o _id.</summary>
[BsonIgnoreExtraElements]
internal sealed class MongoProductDocument
{
    [BsonId] public string Sku { get; set; } = default!;
    [BsonElement("name")] public string Name { get; set; } = default!;
    [BsonElement("activeIngredient")] public string ActiveIngredient { get; set; } = default!;
    [BsonElement("brand")] public string Brand { get; set; } = default!;
    [BsonElement("category")] public string Category { get; set; } = default!;
    [BsonElement("description")] public string Description { get; set; } = default!;
    [BsonElement("price"), BsonRepresentation(BsonType.Decimal128)] public decimal Price { get; set; }

    /// <summary>
    /// Campo extra só para a estratégia $regex: texto sem acento e minúsculo.
    /// É o que um time costuma criar quando tenta resolver busca com LIKE '%termo%'.
    /// </summary>
    [BsonElement("searchText")] public string SearchText { get; set; } = default!;

    public static MongoProductDocument From(Product product) => new()
    {
        Sku = product.Sku.Value,
        Name = product.Name,
        ActiveIngredient = product.ActiveIngredient,
        Brand = product.Brand,
        Category = product.Category,
        Description = product.Description,
        Price = product.Price,
        SearchText = product.SearchableText
    };
}

/// <summary>Linha devolvida pela busca, com a nota do $text quando existir.</summary>
[BsonIgnoreExtraElements]
internal sealed class MongoSearchRow
{
    [BsonId] public string Sku { get; set; } = default!;
    [BsonElement("name")] public string Name { get; set; } = default!;
    [BsonElement("activeIngredient")] public string ActiveIngredient { get; set; } = default!;
    [BsonElement("brand")] public string Brand { get; set; } = default!;
    [BsonElement("category")] public string Category { get; set; } = default!;
    [BsonElement("price"), BsonRepresentation(BsonType.Decimal128)] public decimal Price { get; set; }
    [BsonElement("score")] public double Score { get; set; }
}
