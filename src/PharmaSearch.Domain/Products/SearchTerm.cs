using PharmaSearch.Domain.Common;

namespace PharmaSearch.Domain.Products;

/// <summary>
/// Value object do termo pesquisado. Garante as invariantes antes de qualquer
/// motor de busca receber a consulta.
/// </summary>
public readonly record struct SearchTerm
{
    public const int MinLength = 2;
    public const int MaxLength = 100;

    /// <summary>Texto como o cliente digitou (sem espaços nas pontas).</summary>
    public string Raw { get; }

    /// <summary>Texto sem acento e em minúsculas.</summary>
    public string Normalized { get; }

    public IReadOnlyList<string> Tokens { get; }

    private SearchTerm(string raw, string normalized)
    {
        Raw = raw;
        Normalized = normalized;
        Tokens = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    public static SearchTerm Create(string? value)
    {
        var raw = value?.Trim() ?? string.Empty;
        if (raw.Length < MinLength)
            throw new DomainException($"A pesquisa precisa de pelo menos {MinLength} caracteres.");
        if (raw.Length > MaxLength)
            throw new DomainException($"A pesquisa aceita no máximo {MaxLength} caracteres.");

        return new SearchTerm(raw, TextNormalizer.Normalize(raw));
    }

    public override string ToString() => Raw;
}
