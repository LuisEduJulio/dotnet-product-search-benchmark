using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PharmaSearch.Application.Abstractions;

namespace PharmaSearch.Infrastructure.Mongo;

public static class DependencyInjection
{
    public static IServiceCollection AddMongoProductSearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoOptions>(configuration.GetSection(MongoOptions.SectionName));

        services.AddSingleton<MongoCatalogContext>();
        services.AddSingleton<IProductSearchEngine, MongoTextProductSearchEngine>();
        services.AddSingleton<IProductSearchEngine, MongoRegexProductSearchEngine>();
        services.AddSingleton<IProductCatalogWriter, MongoCatalogWriter>();

        return services;
    }
}
