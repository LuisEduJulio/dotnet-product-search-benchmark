using PharmaSearch.Domain.Common;
using PharmaSearch.Domain.Products;

namespace PharmaSearch.UnitTests.Domain;

public class SearchTermTests
{
    [Theory]
    [InlineData("  Ácido   Acetilsalicílico ", "acido acetilsalicilico")]
    [InlineData("DIPIRONA", "dipirona")]
    [InlineData("Vitamina C", "vitamina c")]
    public void Normaliza_acento_caixa_e_espacos(string input, string expected)
    {
        var term = SearchTerm.Create(input);

        Assert.Equal(expected, term.Normalized);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" a ")]
    public void Rejeita_termo_curto_demais(string? input)
    {
        Assert.Throws<DomainException>(() => SearchTerm.Create(input));
    }

    [Fact]
    public void Rejeita_termo_longo_demais()
    {
        Assert.Throws<DomainException>(() => SearchTerm.Create(new string('a', SearchTerm.MaxLength + 1)));
    }

    [Fact]
    public void Separa_tokens_normalizados()
    {
        var term = SearchTerm.Create("Dipirona  Gotas");

        Assert.Equal(new[] { "dipirona", "gotas" }, term.Tokens.ToArray());
    }
}

public class ProductTests
{
    [Fact]
    public void Cria_produto_valido_com_texto_pesquisavel_normalizado()
    {
        var product = Product.Create("frm-1", "Ácido Acetilsalicílico 100mg", "Ácido Acetilsalicílico",
            "Vivere", "Analgésicos e Antitérmicos", "desc", 9.90m);

        Assert.Equal("FRM-1", product.Sku.Value);
        Assert.Contains("acido acetilsalicilico", product.SearchableText);
        Assert.DoesNotContain("á", product.SearchableText);
    }

    [Theory]
    [InlineData("", "Analgésicos", 10)]
    [InlineData("Dipirona", "", 10)]
    [InlineData("Dipirona", "Analgésicos", 0)]
    public void Rejeita_produto_invalido(string name, string category, decimal price)
    {
        Assert.Throws<DomainException>(() =>
            Product.Create("FRM-1", name, "Dipirona", "Vivere", category, "desc", price));
    }

    [Fact]
    public void Rejeita_sku_vazio()
    {
        Assert.Throws<DomainException>(() => Sku.Create(" "));
    }
}
