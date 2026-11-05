using PharmaSearch.Domain.Common;

namespace PharmaSearch.Application.Abstractions;

public interface ISearchEngineRegistry
{
    IProductSearchEngine Get(SearchEngineKind kind);

    IReadOnlyCollection<SearchEngineKind> Available { get; }
}

/// <summary>Resolve o motor pelo tipo. Recebe todos os motores registrados no DI (DIP).</summary>
public sealed class SearchEngineRegistry : ISearchEngineRegistry
{
    private readonly IReadOnlyDictionary<SearchEngineKind, IProductSearchEngine> _engines;

    public SearchEngineRegistry(IEnumerable<IProductSearchEngine> engines)
    {
        _engines = engines.ToDictionary(e => e.Kind);
    }

    public IReadOnlyCollection<SearchEngineKind> Available => _engines.Keys.ToArray();

    public IProductSearchEngine Get(SearchEngineKind kind) =>
        _engines.TryGetValue(kind, out var engine)
            ? engine
            : throw new DomainException($"Motor de busca '{kind}' não está registrado.");
}
