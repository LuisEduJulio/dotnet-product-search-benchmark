using PharmaSearch.Domain.Common;

namespace PharmaSearch.Domain.Products;

/// <summary>Value object: código do produto no catálogo (ex.: FRM-000123).</summary>
public readonly record struct Sku
{
    public string Value { get; }

    private Sku(string value) => Value = value;

    public static Sku Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("SKU é obrigatório.");

        var trimmed = value.Trim().ToUpperInvariant();
        if (trimmed.Length > 32)
            throw new DomainException("SKU deve ter no máximo 32 caracteres.");

        return new Sku(trimmed);
    }

    public override string ToString() => Value;
}
