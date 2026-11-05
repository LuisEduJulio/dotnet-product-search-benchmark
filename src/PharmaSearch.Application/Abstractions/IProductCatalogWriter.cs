using PharmaSearch.Domain.Products;

namespace PharmaSearch.Application.Abstractions;

/// <summary>Porta de escrita: recria a estrutura e carrega o catálogo no motor.</summary>
public interface IProductCatalogWriter
{
    /// <summary>Nome da loja de dados (um writer do Mongo atende os dois motores Mongo).</summary>
    string StoreName { get; }

    Task ResetAsync(CancellationToken cancellationToken);

    Task IndexAsync(IReadOnlyCollection<Product> products, CancellationToken cancellationToken);

    /// <summary>Garante que tudo o que foi gravado já está visível para a busca.</summary>
    Task MakeSearchableAsync(CancellationToken cancellationToken);
}
