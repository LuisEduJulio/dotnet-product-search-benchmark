using PharmaSearch.Domain.Common;
using PharmaSearch.Domain.Products;

namespace PharmaSearch.Application.Search;

/// <summary>O que o cliente pediu: termo, filtro opcional de categoria e tamanho da página.</summary>
public sealed record ProductSearchCriteria
{
    public const int DefaultSize = 20;
    public const int MaxSize = 50;

    public SearchTerm Term { get; }
    public string? Category { get; }
    public int Size { get; }

    public ProductSearchCriteria(SearchTerm term, string? category = null, int size = DefaultSize)
    {
        if (size is < 1 or > MaxSize)
            throw new DomainException($"O tamanho da página deve ficar entre 1 e {MaxSize}.");

        Term = term;
        Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        Size = size;
    }
}
