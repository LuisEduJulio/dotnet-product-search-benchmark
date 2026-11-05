using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using PharmaSearch.Application.Catalog;

namespace PharmaSearch.Benchmarks;

public sealed record EnvironmentInfo(
    string GeneratedAtUtc,
    string Os,
    int LogicalCores,
    string DotNet,
    string ElasticsearchVersion,
    string MongoVersion,
    int CatalogSize,
    int Requests,
    int Concurrency);

/// <summary>Grava results/LOAD.md e results/LOAD.json. Nenhum número é digitado à mão.</summary>
public static class ReportWriter
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public static async Task WriteAsync(string outputPath, EnvironmentInfo env,
        IReadOnlyList<IndexCatalogResult> indexing, IReadOnlyList<LoadResult> load)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(outputPath))!;
        Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(Path.ChangeExtension(outputPath, ".json"),
            JsonSerializer.Serialize(new { environment = env, indexing, load }, Json));

        await File.WriteAllTextAsync(outputPath, BuildMarkdown(env, indexing, load));
    }

    private static string N(double value, int decimals = 1) => value.ToString($"N{decimals}", PtBr);

    private static string Pct(double value) => (value * 100).ToString("N0", PtBr) + "%";

    private static string BuildMarkdown(EnvironmentInfo env, IReadOnlyList<IndexCatalogResult> indexing,
        IReadOnlyList<LoadResult> load)
    {
        var md = new StringBuilder();
        md.AppendLine("# Teste de carga");
        md.AppendLine();
        md.AppendLine("### Ambiente");
        md.AppendLine();
        md.AppendLine($"- **Data (UTC):** {env.GeneratedAtUtc}");
        md.AppendLine($"- **Máquina:** {env.Os}, {env.LogicalCores} núcleos lógicos, {env.DotNet}");
        md.AppendLine($"- **Motores:** Elasticsearch {env.ElasticsearchVersion} (heap 1 GB) e MongoDB {env.MongoVersion} (cache WiredTiger 1 GB), na mesma máquina");
        md.AppendLine($"- **Catálogo:** {env.CatalogSize.ToString("N0", PtBr)} produtos fictícios de farmácia, gerados com semente fixa");
        md.AppendLine($"- **Carga:** {env.Requests.ToString("N0", PtBr)} chamadas por cenário e motor, {env.Concurrency} simultâneas, página de 20 itens");
        md.AppendLine();

        md.AppendLine("### Indexação (carga inicial do catálogo)");
        md.AppendLine();
        md.AppendLine("| Loja | Produtos | Tempo | Produtos/s |");
        md.AppendLine("|---|---:|---:|---:|");
        foreach (var i in indexing)
            md.AppendLine($"| {i.Store} | {i.Products.ToString("N0", PtBr)} | {N(i.ElapsedMilliseconds / 1000, 2)} s | {N(i.Products / (i.ElapsedMilliseconds / 1000), 0)} |");
        md.AppendLine();

        md.AppendLine("### Busca sob carga");
        md.AppendLine();
        md.AppendLine("Precisão: quantos dos itens devolvidos são a resposta certa. Cobertura: quanto da página de 20 foi preenchida com respostas certas.");
        md.AppendLine();

        foreach (var group in load.GroupBy(r => r.Scenario))
        {
            var first = group.First();
            var filter = first.Category is null ? "" : $" + categoria \"{first.Category}\"";
            md.AppendLine($"#### {first.Title}: \"{first.Query}\"{filter}");
            md.AppendLine();
            md.AppendLine("| Motor | req/s | p50 | p95 | p99 | Itens | Precisão | Cobertura | Erros |");
            md.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|");
            foreach (var r in group)
                md.AppendLine($"| {r.Engine} | {N(r.RequestsPerSecond, 0)} | {N(r.P50Ms, 2)} ms | {N(r.P95Ms, 2)} ms | {N(r.P99Ms, 2)} ms | {r.Returned} | {Pct(r.Precision)} | {Pct(r.Recall)} | {r.Errors} |");
            md.AppendLine();

            var fastest = group.MaxBy(r => r.RequestsPerSecond)!;
            foreach (var other in group.Where(r => r != fastest && r.RequestsPerSecond > 0))
                md.AppendLine($"- {fastest.Engine} atendeu {N(fastest.RequestsPerSecond / other.RequestsPerSecond, 1)}x as requisições por segundo do {other.Engine}.");
            md.AppendLine();
        }

        return md.ToString();
    }

    public static string DescribeOs() => $"{RuntimeInformation.OSDescription.Trim()} {RuntimeInformation.OSArchitecture}";
}
