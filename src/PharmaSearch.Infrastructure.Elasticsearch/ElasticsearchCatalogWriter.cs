using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PharmaSearch.Application.Abstractions;
using PharmaSearch.Domain.Products;

namespace PharmaSearch.Infrastructure.Elasticsearch;

/// <summary>Cria o índice com o mapeamento e carrega os produtos pela Bulk API.</summary>
internal sealed class ElasticsearchCatalogWriter(HttpClient http, IOptions<ElasticsearchOptions> options)
    : IProductCatalogWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private string Index => options.Value.IndexName;

    public string StoreName => "Elasticsearch";

    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        using (var delete = await http.DeleteAsync($"/{Index}", cancellationToken))
        {
            if (delete.StatusCode != HttpStatusCode.NotFound)
                await ElasticsearchHttp.EnsureSuccessAsync(delete, cancellationToken);
        }

        using var content = new StringContent(ProductIndexDefinition.Json, Encoding.UTF8, "application/json");
        using var create = await http.PutAsync($"/{Index}", content, cancellationToken);
        await ElasticsearchHttp.EnsureSuccessAsync(create, cancellationToken);
    }

    public async Task IndexAsync(IReadOnlyCollection<Product> products, CancellationToken cancellationToken)
    {
        if (products.Count == 0)
            return;

        var ndjson = new StringBuilder(products.Count * 400);
        foreach (var product in products)
        {
            ndjson.Append("{\"index\":{\"_index\":\"").Append(Index)
                  .Append("\",\"_id\":\"").Append(product.Sku.Value).Append("\"}}\n");
            ndjson.Append(JsonSerializer.Serialize(ElasticsearchProductDocument.From(product), JsonOptions)).Append('\n');
        }

        using var content = new StringContent(ndjson.ToString(), Encoding.UTF8, "application/x-ndjson");
        using var response = await http.PostAsync("/_bulk", content, cancellationToken);
        await ElasticsearchHttp.EnsureSuccessAsync(response, cancellationToken);

        using var json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        if (json.RootElement.GetProperty("errors").GetBoolean())
            throw new InvalidOperationException("A Bulk API do Elasticsearch retornou erros ao indexar o lote.");
    }

    public async Task MakeSearchableAsync(CancellationToken cancellationToken)
    {
        using var response = await http.PostAsync($"/{Index}/_refresh", content: null, cancellationToken);
        await ElasticsearchHttp.EnsureSuccessAsync(response, cancellationToken);
    }
}
