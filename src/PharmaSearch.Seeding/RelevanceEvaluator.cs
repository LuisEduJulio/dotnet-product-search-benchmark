using PharmaSearch.Domain.Products;

namespace PharmaSearch.Seeding;

public sealed record RelevanceScore(int Returned, int Relevant, double Precision, double Recall);

/// <summary>
/// Compara os SKUs devolvidos com o gabarito do cenário.
/// Precisão: quantos dos devolvidos são a resposta certa.
/// Cobertura (recall@k): quanto da página ideal foi preenchida com respostas certas.
/// </summary>
public sealed class RelevanceEvaluator
{
    private readonly IReadOnlyDictionary<string, Product> _catalog;

    public RelevanceEvaluator(IEnumerable<Product> catalog)
    {
        _catalog = catalog.ToDictionary(p => p.Sku.Value);
    }

    public RelevanceScore Evaluate(SearchScenario scenario, IReadOnlyList<string> returnedSkus, int pageSize)
    {
        var relevantInCatalog = _catalog.Values.Count(p => scenario.IsRelevant(p) && InCategory(scenario, p));
        var relevantReturned = returnedSkus.Count(sku => _catalog.TryGetValue(sku, out var p) && scenario.IsRelevant(p));

        var precision = returnedSkus.Count == 0 ? 0 : (double)relevantReturned / returnedSkus.Count;
        var idealPage = Math.Min(pageSize, relevantInCatalog);
        var recall = idealPage == 0 ? 0 : (double)relevantReturned / idealPage;

        return new RelevanceScore(returnedSkus.Count, relevantReturned, Math.Round(precision, 3), Math.Round(recall, 3));
    }

    private static bool InCategory(SearchScenario scenario, Product product) =>
        scenario.Category is null || product.Category == scenario.Category;
}
