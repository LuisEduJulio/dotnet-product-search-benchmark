using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PharmaSearch.Application.Abstractions;

namespace PharmaSearch.Infrastructure.Elasticsearch;

public static class DependencyInjection
{
    private const string HttpClientName = "elasticsearch";

    public static IServiceCollection AddElasticsearchProductSearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ElasticsearchOptions>(configuration.GetSection(ElasticsearchOptions.SectionName));

        services.AddHttpClient(HttpClientName, (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ElasticsearchOptions>>().Value;
            client.BaseAddress = new Uri(options.Url);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            // Pool generoso para o teste de carga não medir fila de conexões em vez do motor.
            MaxConnectionsPerServer = 256,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        });

        services.AddSingleton<IProductSearchEngine>(sp => new ElasticsearchProductSearchEngine(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName),
            sp.GetRequiredService<IOptions<ElasticsearchOptions>>()));

        services.AddSingleton<IProductCatalogWriter>(sp => new ElasticsearchCatalogWriter(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName),
            sp.GetRequiredService<IOptions<ElasticsearchOptions>>()));

        return services;
    }
}
