using System.Net;
using System.Text.Json;
using PharmaSearch.Seeding;

namespace PharmaSearch.IntegrationTests;

[Collection(SearchApiCollection.Name)]
public sealed class SearchApiTests(SearchApiFixture api)
{
    public static TheoryData<string> AllEngines => new() { "Elasticsearch", "MongoText", "MongoRegex" };

    private async Task<RelevanceScore> ScoreAsync(string engine, SearchScenario scenario)
    {
        var response = await api.SearchAsync(engine, scenario.Query, scenario.Category);
        return api.Evaluator.Evaluate(scenario, response.Items.Select(i => i.Sku).ToList(), pageSize: 20);
    }

    [Theory]
    [MemberData(nameof(AllEngines))]
    public async Task Termo_exato_traz_o_produto_certo_em_todos_os_motores(string engine)
    {
        var score = await ScoreAsync(engine, SearchScenarios.ExactTerm);

        Assert.Equal(20, score.Returned);
        Assert.True(score.Precision >= 0.9, $"{engine}: precisão {score.Precision}");
    }

    [Theory]
    [MemberData(nameof(AllEngines))]
    public async Task Busca_sem_acento_encontra_produto_com_acento(string engine)
    {
        var score = await ScoreAsync(engine, SearchScenarios.WithoutAccent);

        Assert.True(score.Returned > 0, $"{engine} não devolveu nada para 'acido acetilsalicilico'.");
        Assert.True(score.Precision >= 0.9, $"{engine}: precisão {score.Precision}");
    }

    [Theory]
    [MemberData(nameof(AllEngines))]
    public async Task Filtro_de_categoria_e_respeitado(string engine)
    {
        var scenario = SearchScenarios.CategoryFilter;
        var response = await api.SearchAsync(engine, scenario.Query, scenario.Category);

        Assert.True(response.Count > 0);
        Assert.All(response.Items, item => Assert.Equal(scenario.Category, item.Category));
    }

    [Fact]
    public async Task Elasticsearch_tolera_erro_de_digitacao()
    {
        var score = await ScoreAsync("Elasticsearch", SearchScenarios.Typo);

        Assert.True(score.Recall >= 0.9, $"recall {score.Recall} para 'dipirna'");
    }

    [Theory]
    [InlineData("MongoText")]
    [InlineData("MongoRegex")]
    public async Task Mongo_nao_encontra_termo_com_erro_de_digitacao(string engine)
    {
        // Teste de caracterização: documenta o comportamento, não é um defeito do código.
        var response = await api.SearchAsync(engine, SearchScenarios.Typo.Query);

        Assert.Equal(0, response.Count);
    }

    [Theory]
    [InlineData("Elasticsearch")]
    [InlineData("MongoRegex")]
    public async Task Autocomplete_por_prefixo_funciona(string engine)
    {
        var score = await ScoreAsync(engine, SearchScenarios.Prefix);

        Assert.True(score.Returned > 0);
        Assert.True(score.Precision >= 0.9, $"{engine}: precisão {score.Precision}");
    }

    [Fact]
    public async Task Indice_de_texto_do_Mongo_nao_busca_por_prefixo()
    {
        var response = await api.SearchAsync("MongoText", SearchScenarios.Prefix.Query);

        Assert.Equal(0, response.Count);
    }

    [Fact]
    public async Task Todos_os_motores_devolvem_o_mesmo_contrato_json()
    {
        var shapes = new List<string>();

        foreach (var engine in new[] { "Elasticsearch", "MongoText", "MongoRegex" })
        {
            var json = await api.Client.GetStringAsync($"/products/search?engine={engine}&q=dipirona&size=1");
            using var document = JsonDocument.Parse(json);

            var root = string.Join(",", document.RootElement.EnumerateObject().Select(p => p.Name));
            var item = string.Join(",", document.RootElement.GetProperty("items")[0].EnumerateObject().Select(p => p.Name));
            shapes.Add($"{root}|{item}");
        }

        Assert.Single(shapes.Distinct());
    }

    [Theory]
    [InlineData("/products/search?engine=Elasticsearch&q=a")]
    [InlineData("/products/search?engine=Oracle&q=dipirona")]
    [InlineData("/products/search?engine=MongoText&q=dipirona&size=500")]
    public async Task Entrada_invalida_devolve_400(string url)
    {
        using var response = await api.Client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
