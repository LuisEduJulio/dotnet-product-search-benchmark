using System.Text.Json.Serialization;
using PharmaSearch.Domain.Products;

namespace PharmaSearch.Infrastructure.Elasticsearch;

/// <summary>Modelo de persistência do Elasticsearch. O domínio não conhece este formato.</summary>
internal sealed record ElasticsearchProductDocument(
    [property: JsonPropertyName("sku")] string Sku,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("activeIngredient")] string ActiveIngredient,
    [property: JsonPropertyName("brand")] string Brand,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("price")] decimal Price)
{
    public static ElasticsearchProductDocument From(Product product) => new(
        product.Sku.Value, product.Name, product.ActiveIngredient, product.Brand,
        product.Category, product.Description, product.Price);
}
