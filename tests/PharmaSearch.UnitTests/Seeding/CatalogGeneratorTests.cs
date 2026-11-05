using PharmaSearch.Seeding;

namespace PharmaSearch.UnitTests.Seeding;

public class CatalogGeneratorTests
{
    [Fact]
    public void Mesmo_seed_gera_o_mesmo_catalogo()
    {
        var first = PharmacyCatalogGenerator.Generate(2_000);
        var second = PharmacyCatalogGenerator.Generate(2_000);

        Assert.Equal(first.Select(p => p.Name), second.Select(p => p.Name));
        Assert.Equal(2_000, first.Select(p => p.Sku.Value).Distinct().Count());
    }

    [Fact]
    public void Todo_cenario_tem_resposta_certa_no_catalogo()
    {
        var catalog = PharmacyCatalogGenerator.Generate(5_000);

        Assert.All(SearchScenarios.All, scenario =>
            Assert.True(catalog.Count(scenario.IsRelevant) >= 20, $"Cenário {scenario.Key} tem poucos produtos relevantes."));
    }

    [Fact]
    public void Avaliador_calcula_precisao_e_cobertura()
    {
        var catalog = PharmacyCatalogGenerator.Generate(5_000);
        var evaluator = new RelevanceEvaluator(catalog);
        var scenario = SearchScenarios.ExactTerm;

        var relevant = catalog.Where(scenario.IsRelevant).Take(15).Select(p => p.Sku.Value);
        var noise = catalog.Where(p => !scenario.IsRelevant(p)).Take(5).Select(p => p.Sku.Value);

        var score = evaluator.Evaluate(scenario, relevant.Concat(noise).ToList(), pageSize: 20);

        Assert.Equal(20, score.Returned);
        Assert.Equal(15, score.Relevant);
        Assert.Equal(0.75, score.Precision);
        Assert.Equal(0.75, score.Recall);
    }

    [Fact]
    public void Sem_resultado_tem_precisao_e_cobertura_zero()
    {
        var evaluator = new RelevanceEvaluator(PharmacyCatalogGenerator.Generate(1_000));

        var score = evaluator.Evaluate(SearchScenarios.Typo, [], pageSize: 20);

        Assert.Equal(0d, score.Precision);
        Assert.Equal(0d, score.Recall);
    }
}
