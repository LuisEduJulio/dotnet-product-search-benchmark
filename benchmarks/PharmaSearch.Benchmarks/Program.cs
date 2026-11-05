using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using PharmaSearch.Application.Catalog;
using PharmaSearch.Benchmarks;
using PharmaSearch.Seeding;

// Uso:
//   dotnet run -c Release --project benchmarks/PharmaSearch.Benchmarks -- --load --requests 2000 --concurrency 50 --out results/LOAD.md
//   dotnet run -c Release --project benchmarks/PharmaSearch.Benchmarks -- --filter '*'      (BenchmarkDotNet)
//
// --load carrega o catálogo nos dois motores, roda o teste de carga e grava o relatório.
// Sem --load, os argumentos vão direto para o BenchmarkDotNet (o catálogo precisa estar carregado).
if (args.Contains("--load"))
{
    var requests = ReadInt(args, "--requests", BenchmarkHost.LoadRequests);
    var concurrency = ReadInt(args, "--concurrency", BenchmarkHost.LoadConcurrency);
    var output = ReadString(args, "--out", "results/LOAD.md");

    using var services = BenchmarkHost.BuildServices();
    var catalog = PharmacyCatalogGenerator.Generate(BenchmarkHost.CatalogSize);

    Console.WriteLine($"Carregando {catalog.Count:N0} produtos no Elasticsearch e no MongoDB...");
    var indexing = await services.GetRequiredService<IndexCatalogHandler>().HandleAsync(catalog, CancellationToken.None);
    foreach (var i in indexing)
        Console.WriteLine($"  {i.Store}: {i.ElapsedMilliseconds:N0} ms");

    Console.WriteLine($"Teste de carga: {requests} chamadas por cenário e motor, {concurrency} simultâneas.");
    var load = await new LoadTestRunner(services, catalog).RunAsync(requests, concurrency, CancellationToken.None);

    var env = new EnvironmentInfo(
        DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"),
        ReportWriter.DescribeOs(),
        Environment.ProcessorCount,
        RuntimeInformation.FrameworkDescription,
        await GetElasticsearchVersionAsync(),
        await GetMongoVersionAsync(),
        catalog.Count,
        requests,
        concurrency);

    await ReportWriter.WriteAsync(output, env, indexing, load);
    Console.WriteLine($"Relatório gravado em {Path.GetFullPath(output)}");
    return;
}

BenchmarkSwitcher.FromAssembly(typeof(SearchLatencyBenchmarks).Assembly).Run(args);

static int ReadInt(string[] args, string name, int fallback)
{
    var index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length && int.TryParse(args[index + 1], out var value) ? value : fallback;
}

static string ReadString(string[] args, string name, string fallback)
{
    var index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : fallback;
}

static async Task<string> GetElasticsearchVersionAsync()
{
    using var http = new HttpClient { BaseAddress = new Uri(BenchmarkHost.Configuration["Elasticsearch:Url"]!) };
    var root = await http.GetFromJsonAsync<JsonElement>("/");
    return root.GetProperty("version").GetProperty("number").GetString() ?? "?";
}

static async Task<string> GetMongoVersionAsync()
{
    var client = new MongoClient(BenchmarkHost.Configuration["Mongo:ConnectionString"]);
    var info = await client.GetDatabase("admin").RunCommandAsync<BsonDocument>(new BsonDocument("buildInfo", 1));
    return info["version"].AsString;
}
