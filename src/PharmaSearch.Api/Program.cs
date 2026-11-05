using System.Text.Json.Serialization;
using PharmaSearch.Application.Abstractions;
using PharmaSearch.Application.Catalog;
using PharmaSearch.Application.Search;
using PharmaSearch.Domain.Common;
using PharmaSearch.Infrastructure.Elasticsearch;
using PharmaSearch.Infrastructure.Mongo;
using PharmaSearch.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Composition root: o único lugar que conhece as implementações concretas (DIP).
builder.Services
    .AddElasticsearchProductSearch(builder.Configuration)
    .AddMongoProductSearch(builder.Configuration)
    .AddSingleton<ISearchEngineRegistry, SearchEngineRegistry>()
    .AddSingleton<SearchProductsHandler>()
    .AddSingleton<IndexCatalogHandler>();

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// GET /products/search?q=dipirona&engine=elasticsearch&category=...&size=20
app.MapGet("/products/search", async (string? q, string? engine, string? category, int? size,
    SearchProductsHandler handler, CancellationToken ct) =>
{
    if (!Enum.TryParse<SearchEngineKind>(engine ?? nameof(SearchEngineKind.Elasticsearch), ignoreCase: true, out var kind))
        return Results.Problem($"engine inválido. Use: {string.Join(", ", Enum.GetNames<SearchEngineKind>())}.",
            statusCode: StatusCodes.Status400BadRequest);

    try
    {
        var response = await handler.HandleAsync(
            new SearchProductsQuery(q, kind, category, size ?? ProductSearchCriteria.DefaultSize), ct);
        return Results.Ok(response);
    }
    catch (DomainException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
    }
});

// POST /catalog/seed?count=100000 → gera o catálogo e carrega nos dois motores.
app.MapPost("/catalog/seed", async (int? count, IndexCatalogHandler handler, CancellationToken ct) =>
{
    var products = PharmacyCatalogGenerator.Generate(count ?? 100_000);
    var result = await handler.HandleAsync(products, ct);
    return Results.Ok(result);
});

app.Run();

/// <summary>Exposto para os testes de integração (WebApplicationFactory).</summary>
public partial class Program;
