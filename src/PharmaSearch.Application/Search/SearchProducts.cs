using System.Diagnostics;
using PharmaSearch.Application.Abstractions;
using PharmaSearch.Domain.Products;

namespace PharmaSearch.Application.Search;

public sealed record SearchProductsQuery(string? Text, SearchEngineKind Engine, string? Category = null,
    int Size = ProductSearchCriteria.DefaultSize);

public sealed record SearchProductsResponse(
    string Engine,
    string Query,
    string? Category,
    double ElapsedMilliseconds,
    int Count,
    IReadOnlyList<ProductHit> Items);

/// <summary>
/// Caso de uso: pesquisar produtos. Responsabilidade única: validar a entrada pelo domínio,
/// delegar ao motor escolhido e medir o tempo da chamada.
/// </summary>
public sealed class SearchProductsHandler(ISearchEngineRegistry registry)
{
    public async Task<SearchProductsResponse> HandleAsync(SearchProductsQuery query, CancellationToken cancellationToken)
    {
        var criteria = new ProductSearchCriteria(SearchTerm.Create(query.Text), query.Category, query.Size);
        var engine = registry.Get(query.Engine);

        var started = Stopwatch.GetTimestamp();
        var hits = await engine.SearchAsync(criteria, cancellationToken);
        var elapsed = Stopwatch.GetElapsedTime(started);

        return new SearchProductsResponse(
            engine.Kind.ToString(),
            criteria.Term.Raw,
            criteria.Category,
            Math.Round(elapsed.TotalMilliseconds, 3),
            hits.Count,
            hits);
    }
}
