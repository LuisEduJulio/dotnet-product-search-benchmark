using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using PharmaSearch.Application.Abstractions;
using PharmaSearch.Application.Search;

namespace PharmaSearch.Infrastructure.Elasticsearch;

/// <summary>
/// Busca de texto no Elasticsearch. Duas cláusulas "should":
/// - multi_match com fuzziness AUTO (tolera erro de digitação) e pesos por campo (BM25);
/// - multi_match nos subcampos autocomplete (prefixo: "parace" → paracetamol).
/// O filtro de categoria vai em "filter": restringe sem mexer na nota e pode ir para o cache.
/// </summary>
internal sealed class ElasticsearchProductSearchEngine(HttpClient http, IOptions<ElasticsearchOptions> options)
    : IProductSearchEngine
{
    private static readonly string[] SourceFields = ["sku", "name", "activeIngredient", "brand", "category", "price"];

    public SearchEngineKind Kind => SearchEngineKind.Elasticsearch;

    public async Task<IReadOnlyList<ProductHit>> SearchAsync(ProductSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var body = BuildQuery(criteria);
        using var content = new StringContent(body.ToJsonString(), System.Text.Encoding.UTF8, "application/json");
        using var response = await http.PostAsync($"/{options.Value.IndexName}/_search", content, cancellationToken);
        await ElasticsearchHttp.EnsureSuccessAsync(response, cancellationToken);

        using var json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

        var hits = json.RootElement.GetProperty("hits").GetProperty("hits");
        var result = new List<ProductHit>(hits.GetArrayLength());

        foreach (var hit in hits.EnumerateArray())
        {
            var source = hit.GetProperty("_source");
            result.Add(new ProductHit(
                source.GetProperty("sku").GetString()!,
                source.GetProperty("name").GetString()!,
                source.GetProperty("activeIngredient").GetString() ?? string.Empty,
                source.GetProperty("brand").GetString() ?? string.Empty,
                source.GetProperty("category").GetString()!,
                source.GetProperty("price").GetDecimal(),
                hit.TryGetProperty("_score", out var score) && score.ValueKind == JsonValueKind.Number
                    ? Math.Round(score.GetDouble(), 4)
                    : 0d));
        }

        return result;
    }

    internal static JsonObject BuildQuery(ProductSearchCriteria criteria)
    {
        var text = criteria.Term.Raw;

        var should = new JsonArray
        {
            new JsonObject
            {
                ["multi_match"] = new JsonObject
                {
                    ["query"] = text,
                    ["fields"] = new JsonArray("name^3", "activeIngredient^3", "brand^2", "description"),
                    ["fuzziness"] = "AUTO",
                    ["prefix_length"] = 1,
                    ["operator"] = "and"
                }
            },
            new JsonObject
            {
                ["multi_match"] = new JsonObject
                {
                    ["query"] = text,
                    ["fields"] = new JsonArray("name.autocomplete^2", "activeIngredient.autocomplete^2"),
                    ["operator"] = "and"
                }
            }
        };

        var boolQuery = new JsonObject
        {
            ["should"] = should,
            ["minimum_should_match"] = 1
        };

        if (criteria.Category is not null)
        {
            boolQuery["filter"] = new JsonArray(
                new JsonObject { ["term"] = new JsonObject { ["category"] = criteria.Category } });
        }

        var sourceFields = new JsonArray();
        foreach (var field in SourceFields) sourceFields.Add(field);

        return new JsonObject
        {
            ["size"] = criteria.Size,
            ["track_total_hits"] = false,
            ["_source"] = sourceFields,
            ["query"] = new JsonObject { ["bool"] = boolQuery }
        };
    }
}
