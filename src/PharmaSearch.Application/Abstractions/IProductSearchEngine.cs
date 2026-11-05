using PharmaSearch.Application.Search;

namespace PharmaSearch.Application.Abstractions;

/// <summary>
/// Porta de leitura (ISP: separada da escrita). Cada motor é uma estratégia
/// trocável (OCP/LSP): o caso de uso não muda quando entra um motor novo.
/// </summary>
public interface IProductSearchEngine
{
    SearchEngineKind Kind { get; }

    Task<IReadOnlyList<ProductHit>> SearchAsync(ProductSearchCriteria criteria, CancellationToken cancellationToken);
}
