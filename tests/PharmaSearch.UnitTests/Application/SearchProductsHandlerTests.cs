using PharmaSearch.Application.Abstractions;
using PharmaSearch.Application.Search;
using PharmaSearch.Domain.Common;
using PharmaSearch.Domain.Products;

namespace PharmaSearch.UnitTests.Application;

public class SearchProductsHandlerTests
{
    /// <summary>Motor falso: devolve o que recebeu na construção e guarda o critério usado.</summary>
    private sealed class FakeEngine(SearchEngineKind kind, params ProductHit[] hits) : IProductSearchEngine
    {
        public ProductSearchCriteria? LastCriteria { get; private set; }

        public SearchEngineKind Kind => kind;

        public Task<IReadOnlyList<ProductHit>> SearchAsync(ProductSearchCriteria criteria, CancellationToken cancellationToken)
        {
            LastCriteria = criteria;
            return Task.FromResult<IReadOnlyList<ProductHit>>(hits);
        }
    }

    private static readonly ProductHit Dipirona =
        new("FRM-0000001", "Dipirona 500mg", "Dipirona Monoidratada", "Vivere", "Analgésicos e Antitérmicos", 9.9m, 1.5);

    [Fact]
    public async Task Usa_o_motor_pedido_e_devolve_o_contrato_padrao()
    {
        var elastic = new FakeEngine(SearchEngineKind.Elasticsearch, Dipirona);
        var mongo = new FakeEngine(SearchEngineKind.MongoText);
        var handler = new SearchProductsHandler(new SearchEngineRegistry([elastic, mongo]));

        var response = await handler.HandleAsync(
            new SearchProductsQuery("  dipirona ", SearchEngineKind.Elasticsearch, "Analgésicos e Antitérmicos"), CancellationToken.None);

        Assert.Equal("Elasticsearch", response.Engine);
        Assert.Equal("dipirona", response.Query);
        Assert.Equal(1, response.Count);
        Assert.Equal(Dipirona, response.Items[0]);
        Assert.Null(mongo.LastCriteria);
        Assert.Equal("Analgésicos e Antitérmicos", elastic.LastCriteria!.Category);
    }

    [Fact]
    public async Task Termo_invalido_nao_chega_ao_motor()
    {
        var engine = new FakeEngine(SearchEngineKind.MongoRegex);
        var handler = new SearchProductsHandler(new SearchEngineRegistry([engine]));

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new SearchProductsQuery("a", SearchEngineKind.MongoRegex), CancellationToken.None));
        Assert.Null(engine.LastCriteria);
    }

    [Fact]
    public void Motor_nao_registrado_gera_erro_de_dominio()
    {
        var registry = new SearchEngineRegistry([new FakeEngine(SearchEngineKind.Elasticsearch)]);

        Assert.Throws<DomainException>(() => registry.Get(SearchEngineKind.MongoText));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(ProductSearchCriteria.MaxSize + 1)]
    public void Tamanho_de_pagina_fora_do_limite_e_rejeitado(int size)
    {
        Assert.Throws<DomainException>(() => new ProductSearchCriteria(SearchTerm.Create("dipirona"), size: size));
    }
}
