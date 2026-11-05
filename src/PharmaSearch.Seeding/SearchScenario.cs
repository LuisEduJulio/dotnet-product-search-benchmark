using PharmaSearch.Domain.Products;

namespace PharmaSearch.Seeding;

/// <summary>
/// Um cenário de busca com gabarito: sabemos, pelo catálogo gerado,
/// quais produtos são a resposta certa. Assim medimos qualidade, não só tempo.
/// </summary>
public sealed record SearchScenario(
    string Key,
    string Title,
    string Query,
    string? Category,
    Func<Product, bool> IsRelevant)
{
    public override string ToString() => Key;
}

public static class SearchScenarios
{
    private static bool HasIngredient(Product p, string ingredient) =>
        p.ActiveIngredient.Equals(ingredient, StringComparison.Ordinal);

    public static readonly SearchScenario ExactTerm = new(
        "exato", "Termo exato", "dipirona", null,
        p => HasIngredient(p, "Dipirona Monoidratada"));

    public static readonly SearchScenario MultiTerm = new(
        "dois-termos", "Dois termos", "dipirona gotas", null,
        p => HasIngredient(p, "Dipirona Monoidratada") && p.Name.Contains(" Gotas ", StringComparison.Ordinal));

    public static readonly SearchScenario WithoutAccent = new(
        "sem-acento", "Sem acento", "acido acetilsalicilico", null,
        p => HasIngredient(p, "Ácido Acetilsalicílico"));

    public static readonly SearchScenario Typo = new(
        "erro-digitacao", "Erro de digitação", "dipirna", null,
        p => HasIngredient(p, "Dipirona Monoidratada"));

    public static readonly SearchScenario Prefix = new(
        "prefixo", "Autocomplete (prefixo)", "paracet", null,
        p => HasIngredient(p, "Paracetamol"));

    public static readonly SearchScenario CategoryFilter = new(
        "com-filtro", "Termo + filtro de categoria", "vitamina c", PharmacyCatalogGenerator.Vitamins,
        p => HasIngredient(p, "Vitamina C"));

    public static IReadOnlyList<SearchScenario> All { get; } =
        [ExactTerm, MultiTerm, WithoutAccent, Typo, Prefix, CategoryFilter];

    public static SearchScenario ByKey(string key) =>
        All.First(s => s.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
}
