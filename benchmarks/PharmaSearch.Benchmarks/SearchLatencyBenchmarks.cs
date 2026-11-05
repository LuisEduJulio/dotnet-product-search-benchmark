using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using PharmaSearch.Application.Abstractions;
using PharmaSearch.Application.Search;
using PharmaSearch.Seeding;

namespace PharmaSearch.Benchmarks;

/// <summary>
/// Latência de uma chamada por vez (sem concorrência), motor × cenário.
/// O catálogo já foi carregado antes pelo modo "seed"; aqui só lemos.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 15)]
public class SearchLatencyBenchmarks
{
    private ServiceProvider _services = default!;
    private SearchProductsHandler _handler = default!;
    private SearchProductsQuery _query = default!;

    public IEnumerable<SearchEngineKind> Engines => Enum.GetValues<SearchEngineKind>();
    public IEnumerable<string> Scenarios => SearchScenarios.All.Select(s => s.Key);

    [ParamsSource(nameof(Engines))]
    public SearchEngineKind Engine { get; set; }

    [ParamsSource(nameof(Scenarios))]
    public string Scenario { get; set; } = default!;

    [GlobalSetup]
    public void Setup()
    {
        _services = BenchmarkHost.BuildServices();
        _handler = _services.GetRequiredService<SearchProductsHandler>();

        var scenario = SearchScenarios.ByKey(Scenario);
        _query = new SearchProductsQuery(scenario.Query, Engine, scenario.Category);
    }

    [Benchmark]
    public Task<SearchProductsResponse> Search() => _handler.HandleAsync(_query, CancellationToken.None);

    [GlobalCleanup]
    public void Cleanup() => _services.Dispose();
}
