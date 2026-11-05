using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using PharmaSearch.Application.Abstractions;
using PharmaSearch.Application.Search;
using PharmaSearch.Domain.Products;
using PharmaSearch.Seeding;

namespace PharmaSearch.Benchmarks;

public sealed record LoadResult(
    string Scenario,
    string Title,
    string Query,
    string? Category,
    string Engine,
    int Requests,
    int Concurrency,
    int Errors,
    double RequestsPerSecond,
    double MeanMs,
    double P50Ms,
    double P95Ms,
    double P99Ms,
    int Returned,
    double Precision,
    double Recall);

/// <summary>
/// Teste de carga: N chamadas com C simultâneas, para cada cenário × motor.
/// Mede vazão (req/s) e percentis de latência, e avalia a qualidade da resposta
/// comparando com o gabarito do catálogo gerado.
/// </summary>
public sealed class LoadTestRunner(ServiceProvider services, IReadOnlyList<Product> catalog)
{
    private const int WarmupCalls = 50;

    public async Task<IReadOnlyList<LoadResult>> RunAsync(int requests, int concurrency, CancellationToken cancellationToken)
    {
        var handler = services.GetRequiredService<SearchProductsHandler>();
        var evaluator = new RelevanceEvaluator(catalog);
        var results = new List<LoadResult>();

        foreach (var scenario in SearchScenarios.All)
        {
            foreach (var engine in Enum.GetValues<SearchEngineKind>())
            {
                var query = new SearchProductsQuery(scenario.Query, engine, scenario.Category);

                // Aquecimento: conexões abertas, JIT e caches do motor no mesmo estado para todos.
                SearchProductsResponse sample = default!;
                for (var i = 0; i < WarmupCalls; i++)
                    sample = await handler.HandleAsync(query, cancellationToken);

                var relevance = evaluator.Evaluate(scenario, sample.Items.Select(h => h.Sku).ToList(),
                    ProductSearchCriteria.DefaultSize);

                var latencies = new double[requests];
                var errors = new ConcurrentBag<string>();

                var started = Stopwatch.GetTimestamp();
                await Parallel.ForEachAsync(
                    Enumerable.Range(0, requests),
                    new ParallelOptions { MaxDegreeOfParallelism = concurrency, CancellationToken = cancellationToken },
                    async (index, ct) =>
                    {
                        var callStarted = Stopwatch.GetTimestamp();
                        try
                        {
                            await handler.HandleAsync(query, ct);
                        }
                        catch (Exception ex)
                        {
                            errors.Add(ex.GetType().Name);
                        }
                        latencies[index] = Stopwatch.GetElapsedTime(callStarted).TotalMilliseconds;
                    });
                var wall = Stopwatch.GetElapsedTime(started);

                Array.Sort(latencies);
                var result = new LoadResult(
                    scenario.Key, scenario.Title, scenario.Query, scenario.Category, engine.ToString(),
                    requests, concurrency, errors.Count,
                    Math.Round(requests / wall.TotalSeconds, 1),
                    Math.Round(latencies.Average(), 2),
                    Percentile(latencies, 0.50),
                    Percentile(latencies, 0.95),
                    Percentile(latencies, 0.99),
                    relevance.Returned, relevance.Precision, relevance.Recall);

                Console.WriteLine($"[{scenario.Key}] {engine,-13} {result.RequestsPerSecond,8} req/s  " +
                                  $"p95 {result.P95Ms,8} ms  resultados {result.Returned,2}  precisão {result.Precision}");
                results.Add(result);
            }
        }

        return results;
    }

    private static double Percentile(double[] sorted, double percentile)
    {
        if (sorted.Length == 0) return 0;
        var rank = (int)Math.Ceiling(percentile * sorted.Length) - 1;
        return Math.Round(sorted[Math.Clamp(rank, 0, sorted.Length - 1)], 2);
    }
}
