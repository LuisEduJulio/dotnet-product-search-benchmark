using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PharmaSearch.Application.Search;
using PharmaSearch.Domain.Products;
using PharmaSearch.Seeding;

namespace PharmaSearch.IntegrationTests;

/// <summary>
/// Sobe a API uma vez, carrega 5.000 produtos nos dois motores (índice e coleção
/// próprios dos testes) e compartilha com todos os testes da coleção.
/// </summary>
public sealed class SearchApiFixture : IAsyncLifetime
{
    public const int CatalogSize = 5_000;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private WebApplicationFactory<Program> _factory = default!;

    public HttpClient Client { get; private set; } = default!;
    public IReadOnlyList<Product> Catalog { get; } = PharmacyCatalogGenerator.Generate(CatalogSize);
    public RelevanceEvaluator Evaluator { get; }

    public SearchApiFixture()
    {
        Evaluator = new RelevanceEvaluator(Catalog);
    }

    public async Task InitializeAsync()
    {
        // URLs vêm de Elasticsearch__Url e Mongo__ConnectionString (variáveis de ambiente) ou do appsettings.
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Elasticsearch:IndexName", "products_it");
            builder.UseSetting("Mongo:Collection", "products_it");
        });

        Client = _factory.CreateClient();

        using var seed = await Client.PostAsync($"/catalog/seed?count={CatalogSize}", content: null);
        seed.EnsureSuccessStatusCode();
    }

    public async Task<SearchProductsResponse> SearchAsync(string engine, string query, string? category = null)
    {
        var url = $"/products/search?engine={engine}&q={Uri.EscapeDataString(query)}";
        if (category is not null)
            url += $"&category={Uri.EscapeDataString(category)}";

        var response = await Client.GetFromJsonAsync<SearchProductsResponse>(url, JsonOptions);
        return response!;
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class SearchApiCollection : ICollectionFixture<SearchApiFixture>
{
    public const string Name = "search-api";
}
