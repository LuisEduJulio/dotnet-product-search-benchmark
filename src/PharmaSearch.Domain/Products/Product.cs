using PharmaSearch.Domain.Common;

namespace PharmaSearch.Domain.Products;

/// <summary>
/// Aggregate root do catálogo. Não sabe nada de Elasticsearch nem de MongoDB:
/// cada adaptador de infraestrutura tem o seu próprio modelo de persistência.
/// </summary>
public sealed class Product
{
    public Sku Sku { get; }
    public string Name { get; }
    public string ActiveIngredient { get; }
    public string Brand { get; }
    public string Category { get; }
    public string Description { get; }
    public decimal Price { get; }

    private Product(Sku sku, string name, string activeIngredient, string brand,
        string category, string description, decimal price)
    {
        Sku = sku;
        Name = name;
        ActiveIngredient = activeIngredient;
        Brand = brand;
        Category = category;
        Description = description;
        Price = price;
    }

    public static Product Create(string sku, string name, string activeIngredient, string brand,
        string category, string description, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do produto é obrigatório.");
        if (string.IsNullOrWhiteSpace(category))
            throw new DomainException("Categoria é obrigatória.");
        if (price <= 0)
            throw new DomainException("Preço deve ser maior que zero.");

        return new Product(Sku.Create(sku), name.Trim(), activeIngredient?.Trim() ?? string.Empty,
            brand?.Trim() ?? string.Empty, category.Trim(), description?.Trim() ?? string.Empty, price);
    }

    /// <summary>Texto pesquisável já normalizado (sem acento, minúsculo).</summary>
    public string SearchableText =>
        TextNormalizer.Normalize($"{Name} {ActiveIngredient} {Brand} {Category}");
}
