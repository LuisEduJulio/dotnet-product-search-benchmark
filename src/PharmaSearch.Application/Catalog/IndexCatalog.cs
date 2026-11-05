using System.Diagnostics;
using PharmaSearch.Application.Abstractions;
using PharmaSearch.Domain.Products;

namespace PharmaSearch.Application.Catalog;

public sealed record IndexCatalogResult(string Store, int Products, double ElapsedMilliseconds);

/// <summary>
/// Caso de uso: carregar o mesmo catálogo em todas as lojas de dados, em lotes.
/// Mede o tempo de indexação de cada uma (o custo de escrita também entra na conta).
/// </summary>
public sealed class IndexCatalogHandler(IEnumerable<IProductCatalogWriter> writers)
{
    public const int BatchSize = 5_000;

    public async Task<IReadOnlyList<IndexCatalogResult>> HandleAsync(
        IReadOnlyList<Product> products, CancellationToken cancellationToken)
    {
        var results = new List<IndexCatalogResult>();

        foreach (var writer in writers)
        {
            await writer.ResetAsync(cancellationToken);

            var started = Stopwatch.GetTimestamp();
            foreach (var batch in products.Chunk(BatchSize))
                await writer.IndexAsync(batch, cancellationToken);
            await writer.MakeSearchableAsync(cancellationToken);
            var elapsed = Stopwatch.GetElapsedTime(started);

            results.Add(new IndexCatalogResult(writer.StoreName, products.Count, Math.Round(elapsed.TotalMilliseconds, 1)));
        }

        return results;
    }
}
