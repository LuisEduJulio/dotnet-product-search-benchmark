using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PharmaSearch.Application.Abstractions;
using PharmaSearch.Application.Catalog;
using PharmaSearch.Application.Search;
using PharmaSearch.Infrastructure.Elasticsearch;
using PharmaSearch.Infrastructure.Mongo;

namespace PharmaSearch.Benchmarks;

/// <summary>
/// Composition root do benchmark: mesmo registro da API, configurado por variáveis de ambiente
/// (Elasticsearch__Url, Mongo__ConnectionString, CATALOG_SIZE...). Os processos filhos do
/// BenchmarkDotNet herdam essas variáveis.
/// </summary>
public static class BenchmarkHost
{
    public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Elasticsearch:Url"] = "http://localhost:9200",
            ["Elasticsearch:IndexName"] = "products",
            ["Mongo:ConnectionString"] = "mongodb://localhost:27017",
            ["Mongo:Database"] = "catalog",
            ["Mongo:Collection"] = "products",
            ["CATALOG_SIZE"] = "100000",
            ["LOAD_REQUESTS"] = "2000",
            ["LOAD_CONCURRENCY"] = "50"
        })
        .AddEnvironmentVariables()
        .Build();

    public static int CatalogSize => Configuration.GetValue<int>("CATALOG_SIZE");
    public static int LoadRequests => Configuration.GetValue<int>("LOAD_REQUESTS");
    public static int LoadConcurrency => Configuration.GetValue<int>("LOAD_CONCURRENCY");

    public static ServiceProvider BuildServices() =>
        new ServiceCollection()
            .AddElasticsearchProductSearch(Configuration)
            .AddMongoProductSearch(Configuration)
            .AddSingleton<ISearchEngineRegistry, SearchEngineRegistry>()
            .AddSingleton<SearchProductsHandler>()
            .AddSingleton<IndexCatalogHandler>()
            .BuildServiceProvider();
}
